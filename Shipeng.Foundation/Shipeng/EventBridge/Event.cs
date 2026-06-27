using System;
using System.Reflection;
using System.Threading.Tasks;
using Shipeng.Dependency;
using Shipeng.Extensions;
using Shipeng.FriendlyException;
using Shipeng.IPCChannel;
using Shipeng.Reflection;

namespace Shipeng.EventBridge
{
    /// <summary>
    /// 事件总线静态入口。
    ///
    /// 作用：
    /// 1. 统一发布系统内部事件。
    /// 2. 支持通过 “事件分类:事件编号” 发布事件。
    /// 3. 支持通过事件处理器类型自动推导事件分类。
    /// 4. 将事件负载序列化后交给事件存储提供器。
    /// 5. 将事件消息写入内存通道，由 EventDispatcher 异步分发执行。
    ///
    /// 事件编号规则：
    /// 1. 完整事件编号格式必须是：分类:事件Id。
    /// 2. 例如：Merchant:SyncSaveMerchant。
    /// 3. 分类 Merchant 对应事件处理器上的 [EventHandler("Merchant")]。
    /// 4. 事件Id SyncSaveMerchant 对应处理方法上的 [EventMessage("SyncSaveMerchant")]。
    ///
    /// Payload 支持：
    /// 1. 简单类型：string、int、decimal、bool、DateTime、Guid、enum 等。
    /// 2. 普通对象：Entity、DTO、匿名对象。
    /// 3. Tuple：Tuple.Create(entity, null, null)。
    /// 4. ValueTuple：(organize: entity, adopter: null, contractor: null)。
    ///
    /// 注意：
    /// 1. 这里是事件总线基础封装，不写具体业务。
    /// 2. 业务逻辑应该写在具体 EventHandler 里。
    /// 3. Payload 的序列化和兼容转换统一交给 EventPayloadHelper。
    /// </summary>
    [SuppressSniffer]
    public static class Event
    {
        /// <summary>
        /// 同步发布事件。
        ///
        /// 使用场景：
        /// 1. 老代码或非 async 方法中需要发布事件。
        /// 2. 能用 EmitAsync 的地方优先用 EmitAsync。
        ///
        /// 注意：
        /// 该方法内部会阻塞等待异步发布完成，不建议在高并发主链路里大量使用。
        /// </summary>
        /// <param name="eventCombineId">事件组合编号，格式：分类:事件Id，例如 Merchant:SyncSaveMerchant</param>
        /// <param name="payload">事件负载数据，可以为空</param>
        public static void Emit(string eventCombineId, object payload = default)
        {
            EmitAsync(eventCombineId, payload).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步发布事件。
        ///
        /// 执行流程：
        /// 1. 校验并解析事件编号。
        /// 2. 根据事件分类获取事件处理器元数据。
        /// 3. 将 payload 序列化成字符串保存。
        /// 4. 记录 payload 的真实程序集和类型名称。
        /// 5. 追加事件消息，并由事件存储提供器继续写入通道。
        ///
        /// 重要说明：
        /// 1. 如果没有找到事件处理器，直接返回，不影响主业务。
        /// 2. 这样做是为了支持可选事件，不让某个模块没启用时阻断业务流程。
        /// 3. 但是如果你调试事件，一定要确认对应 Handler 是否已注册。
        /// </summary>
        /// <param name="eventCombineId">事件组合编号，格式：分类:事件Id</param>
        /// <param name="payload">事件负载数据，可以为空</param>
        /// <returns></returns>
        public static async Task EmitAsync(string eventCombineId, object payload = default)
        {
            var (category, eventId) = ParseEventCombineId(eventCombineId);

            var eventStoreProvider = App.GetService<IEventStoreProvider>();

            if (eventStoreProvider == null)
                throw Tracexception.Oh("事件总线未注册 IEventStoreProvider，请确认 services.AddEventBridge() 是否已执行。");

            var eventHandlerMetadata = await eventStoreProvider.GetEventHandlerAsync(category);

            if (eventHandlerMetadata == null)
                return;

            var payloadType = payload?.GetType();

            await eventStoreProvider.AppendEventMessageAsync(new EventMessageMetadata
            {
                AssemblyName = eventHandlerMetadata.AssemblyName,
                Category = eventHandlerMetadata.Category,
                TypeFullName = eventHandlerMetadata.TypeFullName,
                EventId = eventId,
                CreatedTime = DateTimeOffset.UtcNow,

                // Payload 为空时不记录类型信息。
                Payload = payload == null ? default : EventPayloadHelper.SerializePayload(payload, payloadType),
                PayloadAssemblyName = payload == null ? default : Reflect.GetAssemblyName(payloadType),
                PayloadTypeFullName = payload == null ? default : payloadType.FullName
            });
        }

        /// <summary>
        /// 根据事件处理器类型异步发布事件。
        ///
        /// 示例：
        /// await Event.EmitAsync&lt;MerchantEventHandler&gt;("SyncSaveMerchant", payload);
        ///
        /// 说明：
        /// 1. 如果 MerchantEventHandler 上有 [EventHandler("Merchant")]，事件分类为 Merchant。
        /// 2. 最终等价于 Event.EmitAsync("Merchant:SyncSaveMerchant", payload)。
        /// </summary>
        /// <typeparam name="TEventHandler">事件处理器类型</typeparam>
        /// <param name="eventId">事件Id，不需要带分类</param>
        /// <param name="payload">事件负载数据</param>
        /// <returns></returns>
        public static Task EmitAsync<TEventHandler>(string eventId, object payload = default)
            where TEventHandler : class, IEventHandler
        {
            return EmitAsync($"{GetEventHandlerCategory(typeof(TEventHandler))}:{eventId}", payload);
        }

        /// <summary>
        /// 根据事件处理器类型同步发布事件。
        /// </summary>
        /// <typeparam name="TEventHandler">事件处理器类型</typeparam>
        /// <param name="eventId">事件Id，不需要带分类</param>
        /// <param name="payload">事件负载数据</param>
        public static void Emit<TEventHandler>(string eventId, object payload = default)
            where TEventHandler : class, IEventHandler
        {
            EmitAsync<TEventHandler>(eventId, payload).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 根据事件消息元数据写入事件通道。
        ///
        /// 使用场景：
        /// 1. IEventStoreProvider.AppendEventMessageAsync 内部调用。
        /// 2. 事件已经被构造成 EventMessageMetadata 后，进入实际分发流程。
        ///
        /// 执行流程：
        /// 1. 根据元数据反序列化 payload。
        /// 2. 构造 EventMessage。
        /// 3. 写入 ChannelContext，由 EventDispatcher 消费并执行处理器。
        /// </summary>
        /// <param name="eventMessageMetadata">事件消息元数据</param>
        /// <returns></returns>
        public static async Task EmitAsync(EventMessageMetadata eventMessageMetadata)
        {
            if (eventMessageMetadata == null)
                return;

            var payload = DeserializePayload(eventMessageMetadata);

            await ChannelContext<EventMessage, EventDispatcher>
                .BoundedChannel
                .Writer
                .WriteAsync(new EventMessage(eventMessageMetadata.Category, eventMessageMetadata.EventId, payload));
        }

        /// <summary>
        /// 根据事件消息元数据反序列化 Payload。
        ///
        /// 说明：
        /// 1. EventMessageMetadata 保存的是字符串 Payload。
        /// 2. PayloadAssemblyName + PayloadTypeFullName 用于恢复原始发送类型。
        /// 3. 具体反序列化细节统一交给 EventPayloadHelper。
        /// 4. Tuple / ValueTuple 的特殊兼容也在 EventPayloadHelper 中处理。
        /// </summary>
        /// <param name="eventMessageMetadata">事件消息元数据</param>
        /// <returns>反序列化后的 payload 对象</returns>
        public static object DeserializePayload(EventMessageMetadata eventMessageMetadata)
        {
            if (eventMessageMetadata == null || eventMessageMetadata.Payload == null)
                return null;

            var payloadType = Reflect.GetType(eventMessageMetadata.PayloadAssemblyName, eventMessageMetadata.PayloadTypeFullName);

            if (payloadType == null)
                throw Tracexception.Oh("事件负载类型解析失败，无法反序列化事件数据。");

            return EventPayloadHelper.DeserializePayload(eventMessageMetadata.Payload.ToString(), payloadType);
        }

        /// <summary>
        /// 获取事件处理器分类名。
        ///
        /// 规则：
        /// 1. 优先读取 [EventHandler("Merchant")] 中配置的分类。
        /// 2. 如果没有配置，则默认取类名去掉 EventHandler 后缀。
        ///
        /// 示例：
        /// 1. MerchantEventHandler + [EventHandler("Merchant")] => Merchant。
        /// 2. TenantEventHandler 没有特性 => Tenant。
        /// </summary>
        /// <param name="type">事件处理器类型</param>
        /// <returns>事件分类名</returns>
        internal static string GetEventHandlerCategory(Type type)
        {
            var defaultCategory = type.Name.ClearStringAffixes(1, "EventHandler");

            return type.IsDefined(typeof(EventHandlerAttribute), false)
                ? type.GetCustomAttribute<EventHandlerAttribute>(false).Category ?? defaultCategory
                : defaultCategory;
        }

        /// <summary>
        /// 解析事件组合编号。
        ///
        /// 格式要求：
        /// 分类:事件Id
        ///
        /// 示例：
        /// Merchant:SyncSaveMerchant
        ///
        /// 返回：
        /// category = Merchant
        /// eventId = SyncSaveMerchant
        /// </summary>
        /// <param name="eventCombineId">事件组合编号</param>
        /// <returns>事件分类和事件Id</returns>
        private static (string category, string eventId) ParseEventCombineId(string eventCombineId)
        {
            if (string.IsNullOrWhiteSpace(eventCombineId))
                throw Tracexception.Oh("事件编号不能为空，正确格式为：分类:事件Id。");

            var arr = eventCombineId.Split(':', StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length != 2)
                throw Tracexception.Oh($"事件编号格式不正确：{eventCombineId}，正确格式为：分类:事件Id。");

            var category = arr[0]?.Trim();
            var eventId = arr[1]?.Trim();

            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(eventId))
                throw Tracexception.Oh($"事件编号格式不正确：{eventCombineId}，分类和事件Id都不能为空。");

            return (category, eventId);
        }
    }
}