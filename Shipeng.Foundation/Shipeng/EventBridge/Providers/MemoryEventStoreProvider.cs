using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Shipeng.Dependency;

namespace Shipeng.EventBridge
{
    /// <summary>
    /// 内存事件存储提供器。
    ///
    /// 作用：
    /// 1. 保存事件处理器元数据。
    /// 2. 保存最近一次事件消息元数据。
    /// 3. 追加事件消息后立即写入事件通道。
    /// 4. 提供事件成功、失败回调入口。
    ///
    /// 说明：
    /// 1. 当前实现是内存版，适合当前项目内部事件联动。
    /// 2. 如果以后要做可靠事件、失败重试、消息持久化，可以新建数据库版 Provider。
    /// 3. 当前 EventMessageStore 使用 category:eventId 作为 key，只保存同类事件最后一次元数据。
    /// 4. 这个设计能满足你现在的同步触发场景，但不适合做历史事件追踪。
    /// </summary>
    [SuppressSniffer]
    public sealed class MemoryEventStoreProvider : IEventStoreProvider
    {
        /// <summary>
        /// 事件处理器存储。
        ///
        /// key：事件分类，例如 Merchant。
        /// value：事件处理器元数据。
        /// </summary>
        private static readonly ConcurrentDictionary<string, EventHandlerMetadata> EventHandlerStore;

        /// <summary>
        /// 事件消息存储。
        ///
        /// key：分类:事件Id，例如 Merchant:SyncSaveMerchant。
        /// value：事件消息元数据。
        ///
        /// 注意：
        /// 当前只保存同一个事件编号的最后一次消息元数据。
        /// 如果以后要完整记录每次事件，需要改成队列或数据库表。
        /// </summary>
        private static readonly ConcurrentDictionary<string, EventMessageMetadata> EventMessageStore;

        /// <summary>
        /// 静态构造函数。
        /// </summary>
        static MemoryEventStoreProvider()
        {
            EventHandlerStore = new ConcurrentDictionary<string, EventHandlerMetadata>();
            EventMessageStore = new ConcurrentDictionary<string, EventMessageMetadata>();
        }

        /// <summary>
        /// 注册事件处理器。
        ///
        /// 说明：
        /// 1. 通常在 AddEventBridge 扫描程序集时调用。
        /// 2. 一个事件分类只允许注册一个事件处理器。
        /// 3. 如果多个处理器使用相同分类，后注册的不会覆盖先注册的。
        /// </summary>
        /// <param name="eventHandlerMetadata">事件处理器元数据</param>
        /// <returns></returns>
        public Task RegisterEventHandlerAsync(EventHandlerMetadata eventHandlerMetadata)
        {
            if (eventHandlerMetadata == null || string.IsNullOrWhiteSpace(eventHandlerMetadata.Category))
                return Task.CompletedTask;

            EventHandlerStore.TryAdd(eventHandlerMetadata.Category, eventHandlerMetadata);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 根据事件分类获取事件处理器元数据。
        /// </summary>
        /// <param name="category">事件分类，例如 Merchant</param>
        /// <returns>事件处理器元数据</returns>
        public Task<EventHandlerMetadata> GetEventHandlerAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return Task.FromResult<EventHandlerMetadata>(default);

            var eventMetadata = EventHandlerStore.TryGetValue(category, out var value)
                ? value
                : default;

            return Task.FromResult(eventMetadata);
        }

        /// <summary>
        /// 追加事件消息。
        ///
        /// 执行流程：
        /// 1. 将事件消息元数据保存到内存字典。
        /// 2. 调用 Event.EmitAsync(EventMessageMetadata) 写入通道。
        /// 3. 由 EventDispatcher 异步消费执行。
        ///
        /// 注意：
        /// 这里不是业务保存，不要在这里写业务逻辑。
        /// </summary>
        /// <param name="eventMessageMetadata">事件消息元数据</param>
        /// <returns></returns>
        public async Task AppendEventMessageAsync(EventMessageMetadata eventMessageMetadata)
        {
            if (eventMessageMetadata == null)
                return;

            var key = BuildEventMessageKey(eventMessageMetadata.Category, eventMessageMetadata.EventId);

            EventMessageStore.AddOrUpdate(key, eventMessageMetadata, (k, oldValue) => eventMessageMetadata);

            await Event.EmitAsync(eventMessageMetadata);
        }

        /// <summary>
        /// 根据事件分类和事件Id获取事件消息元数据。
        /// </summary>
        /// <param name="category">事件分类</param>
        /// <param name="eventId">事件Id</param>
        /// <returns>事件消息元数据</returns>
        public Task<EventMessageMetadata> GetEventMessageAsync(string category, string eventId)
        {
            var key = BuildEventMessageKey(category, eventId);

            var eventMessageMetadata = EventMessageStore.TryGetValue(key, out var value)
                ? value
                : default;

            return Task.FromResult(eventMessageMetadata);
        }

        /// <summary>
        /// 事件执行成功回调。
        ///
        /// 当前内存版不做额外处理。
        /// 如果以后需要记录事件执行日志，可以在这里写入日志表。
        /// </summary>
        /// <param name="eventMessageMetadata">事件消息元数据</param>
        /// <returns></returns>
        public Task ExecuteSuccessfullyAsync(EventMessageMetadata eventMessageMetadata)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 事件执行失败回调。
        ///
        /// 为什么这里必须记录异常：
        /// 1. 你之前遇到“事件没有触发”，实际很多时候是触发了但处理器异常被吞掉。
        /// 2. 如果这里什么都不做，调试时完全看不到失败原因。
        /// 3. 当前先用 Console.Error 输出，至少开发环境能看到。
        /// 4. 后续如果项目有统一日志服务，可以改成写 Serilog 或系统日志表。
        /// </summary>
        /// <param name="eventMessageMetadata">事件消息元数据</param>
        /// <param name="exception">执行异常</param>
        /// <returns></returns>
        public Task ExecuteFaildedAsync(EventMessageMetadata eventMessageMetadata, Exception exception)
        {
            var eventName = eventMessageMetadata == null
                ? "未知事件"
                : $"{eventMessageMetadata.Category}:{eventMessageMetadata.EventId}";

            Console.Error.WriteLine($"事件执行失败：{eventName}");
            Console.Error.WriteLine(exception?.ToString());

            return Task.CompletedTask;
        }

        /// <summary>
        /// 构造事件消息存储 Key。
        /// </summary>
        /// <param name="category">事件分类</param>
        /// <param name="eventId">事件Id</param>
        /// <returns>事件消息 Key</returns>
        private static string BuildEventMessageKey(string category, string eventId)
        {
            return $"{category}:{eventId}";
        }
    }
}