using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Shipeng.Extensions;
using Shipeng.FriendlyException;
using Shipeng.IPCChannel;

namespace Shipeng.EventBridge
{
    /// <summary>
    /// 事件分发调度器。
    ///
    /// 作用：
    /// 1. 从内存通道中消费 EventMessage。
    /// 2. 根据事件分类找到事件处理器。
    /// 3. 根据事件Id找到具体处理方法。
    /// 4. 构造 EventMessage&lt;TPayload&gt; 参数。
    /// 5. 调用事件处理方法。
    /// 6. 调用成功或失败回调，方便后续记录日志、重试、补偿。
    ///
    /// 注意：
    /// 1. 这里是事件总线分发层，不写具体业务。
    /// 2. 具体业务写在 IEventHandler 实现类里。
    /// 3. Payload 转换统一交给 EventPayloadHelper。
    /// </summary>
    internal sealed class EventDispatcher : ChannelHandler<EventMessage>
    {
        /// <summary>
        /// 消费并分发一条事件消息。
        /// </summary>
        /// <param name="eventMessage">通道中读取到的事件消息</param>
        /// <returns></returns>
        public override async Task InvokeAsync(EventMessage eventMessage)
        {
            if (eventMessage == null)
                return;

            var serviceScopeFactory = App.GetService<IServiceScopeFactory>(App.RootServices);

            if (serviceScopeFactory == null)
                throw Tracexception.Oh("事件分发失败：IServiceScopeFactory 未注册。");

            using var scoped = serviceScopeFactory.CreateScope();

            var eventStoreProvider = scoped.ServiceProvider.GetService<IEventStoreProvider>();

            if (eventStoreProvider == null)
                throw Tracexception.Oh("事件分发失败：IEventStoreProvider 未注册。");

            var eventMessageMetadata = await eventStoreProvider.GetEventMessageAsync(eventMessage.Category, eventMessage.EventId);

            if (eventMessageMetadata == null)
                return;

            var eventHandlerResolve = scoped.ServiceProvider.GetService<Func<EventMessageMetadata, IEventHandler>>();

            if (eventHandlerResolve == null)
                throw Tracexception.Oh("事件分发失败：事件处理器解析委托未注册。");

            var eventHandler = eventHandlerResolve(eventMessageMetadata);

            if (eventHandler == null)
                return;

            var methods = FindEventMethods(eventHandler.GetType(), eventMessage.EventId).ToList();

            if (!methods.Any())
                return;

            await InvokeMethodsAsync(methods, eventMessage, eventMessageMetadata, scoped, eventStoreProvider, eventHandler);
        }

        /// <summary>
        /// 查找当前事件需要执行的处理方法。
        ///
        /// 匹配规则：
        /// 1. 方法名去掉 Async 后缀等于 eventId。
        /// 2. 或者方法上有 [EventMessage("eventId")]。
        /// 3. 方法不能是 static。
        /// 4. 返回值必须是 void 或 Task。
        /// 5. 第一个参数必须是 EventMessage&lt;T&gt;。
        ///
        /// 示例：
        /// [EventMessage("SyncSaveMerchant")]
        /// public async Task SyncSaveMerchant(EventMessage&lt;(OrganizeEntity organize, AdopterEntity adopter, ContractorEntity contractor)&gt; eventPayload)
        /// </summary>
        /// <param name="handlerType">事件处理器类型</param>
        /// <param name="eventId">事件Id</param>
        /// <returns>匹配到的方法集合</returns>
        private static IEnumerable<MethodInfo> FindEventMethods(Type handlerType, string eventId)
        {
            return handlerType
                .GetTypeInfo()
                .DeclaredMethods
                .Where(m => !m.IsStatic)
                .Where(m =>
                    m.Name.ClearStringAffixes(1, "Async") == eventId
                    || (m.IsDefined(typeof(EventMessageAttribute), false)
                        && m.GetCustomAttributes<EventMessageAttribute>(false).Any(e => e.EventId == eventId)))
                .Where(m => m.ReturnType == typeof(void) || m.ReturnType == typeof(Task))
                .Where(m =>
                    m.GetParameters().Length > 0
                    && m.GetParameters()[0].ParameterType.HasImplementedRawGeneric(typeof(EventMessage<>)));
        }

        /// <summary>
        /// 执行事件处理方法。
        ///
        /// 说明：
        /// 1. 一个事件可以对应多个处理方法。
        /// 2. 每个方法默认重试 3 次，每次间隔 1 秒。
        /// 3. 如果方法执行过程中抛出异常，则调用 ExecuteFaildedAsync。
        /// 4. 如果 Handler 内部自己写了 catch { } 并吞掉异常，外层无法再捕获异常，只能认为执行成功。
        /// 5. 为了兼容历史代码，这里不会强制要求 Handler 抛异常。
        /// 6. 为了方便排查“事件是否触发”，这里在执行前、执行后、执行失败时都输出调试信息。
        /// </summary>
        private static async Task InvokeMethodsAsync(
            IEnumerable<MethodInfo> methods,
            EventMessage eventMessage,
            EventMessageMetadata eventMessageMetadata,
            IServiceScope scoped,
            IEventStoreProvider eventStoreProvider,
            IEventHandler eventHandler)
        {
            foreach (var method in methods)
            {
                var parameters = new List<object> { ConvertGenericPayload(eventMessage, method) };

                var otherParameters = method.GetParameters().Skip(1);

                foreach (var parameterInfo in otherParameters)
                {
                    if (!parameterInfo.IsDefined(typeof(FromServicesAttribute), false))
                    {
                        parameters.Add(default);
                        continue;
                    }

                    parameters.Add(scoped.ServiceProvider.GetService(parameterInfo.ParameterType));
                }

                try
                {
                    Console.WriteLine($"开始执行事件：{eventMessage.Category}:{eventMessage.EventId}，处理器：{eventHandler.GetType().Name}，方法：{method.Name}");

                    await Retry.Invoke(async () =>
                    {
                        var result = method.Invoke(eventHandler, parameters.ToArray());

                        if (method.IsAsync() && result is Task task)
                            await task;

                    }, 3, 1000, finalThrow: true);

                    Console.WriteLine($"事件执行完成：{eventMessage.Category}:{eventMessage.EventId}，处理器：{eventHandler.GetType().Name}，方法：{method.Name}");

                    await eventStoreProvider.ExecuteSuccessfullyAsync(eventMessageMetadata);
                }
                catch (Exception exception)
                {
                    var realException = exception is TargetInvocationException && exception.InnerException != null
                        ? exception.InnerException
                        : exception;

                    Console.Error.WriteLine($"事件执行异常：{eventMessage.Category}:{eventMessage.EventId}，处理器：{eventHandler.GetType().Name}，方法：{method.Name}");
                    Console.Error.WriteLine(realException.ToString());

                    await eventStoreProvider.ExecuteFaildedAsync(eventMessageMetadata, realException);
                }
            }
        }

        /// <summary>
        /// 构造事件处理方法需要的 EventMessage&lt;TPayload&gt; 参数。
        ///
        /// 说明：
        /// 1. 事件通道中保存的是非泛型 EventMessage。
        /// 2. 事件处理方法需要的是 EventMessage&lt;TPayload&gt;。
        /// 3. 这里根据处理方法第一个参数的泛型类型，重新构造 EventMessage&lt;TPayload&gt;。
        /// 4. 如果发送端 payload 和接收端 TPayload 不一致，交给 EventPayloadHelper 兼容转换。
        ///
        /// 支持：
        /// 1. ValueTuple 发送，ValueTuple 接收。
        /// 2. Tuple 发送，Tuple 接收。
        /// 3. ValueTuple 发送，Tuple 接收。
        /// 4. Tuple 发送，ValueTuple 接收。
        /// 5. 匿名对象发送，DTO/Entity 接收。
        /// </summary>
        /// <param name="eventMessage">原始事件消息</param>
        /// <param name="method">当前事件处理方法</param>
        /// <returns>EventMessage&lt;TPayload&gt; 对象</returns>
        private static object ConvertGenericPayload(EventMessage eventMessage, MethodInfo method)
        {
            var parameterType = method.GetParameters()[0].ParameterType;

            if (!parameterType.IsGenericType)
                return eventMessage;

            var targetPayloadType = parameterType.GetGenericArguments().First();

            var payload = EventPayloadHelper.ConvertPayloadToTargetType(eventMessage.Payload, targetPayloadType);

            return Activator.CreateInstance(
                parameterType,
                new object[] { eventMessage.Category, eventMessage.EventId, payload });
        }
    }
}