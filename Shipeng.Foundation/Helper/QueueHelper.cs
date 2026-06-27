using System.Collections.Concurrent;

namespace Shipeng.Util
{
    /// <summary>
    /// 单消费者后台队列工具。
    /// </summary>
    /// <typeparam name="T">队列元素类型。</typeparam>
    /// <remarks>
    /// 该类型适用于日志写入、轻量通知、非关键路径的数据落库等“生产者快速入队、后台顺序处理”
    /// 的场景。旧实现使用 <see cref="AutoResetEvent"/> 手动唤醒线程，并在释放时向队列写入
    /// <c>default(T)</c> 作为退出信号；当 <typeparamref name="T"/> 本身允许默认值时，这种做法
    /// 容易误判正常数据。本实现改为 <see cref="BlockingCollection{T}"/>，由框架负责阻塞、
    /// 唤醒和完成通知，逻辑更简单，也更适合公共工具库维护。
    /// </remarks>
    public sealed class QueueHelper<T> : IDisposable
    {
        private readonly BlockingCollection<T> _queue = new(new ConcurrentQueue<T>());
        private readonly Task _worker;
        private bool _disposed;

        /// <summary>
        /// 队列元素处理委托。
        /// </summary>
        /// <remarks>
        /// 该委托会在后台单消费者任务中按入队顺序执行。处理逻辑应尽量短小；如果需要执行耗时
        /// I/O，建议在业务层使用异步队列或消息中间件。
        /// </remarks>
        public Action<T> DealAction { get; set; }

        /// <summary>
        /// 创建一个后台队列实例。
        /// </summary>
        public QueueHelper()
        {
            _worker = Task.Run(ProcessQueue);
        }

        /// <summary>
        /// 使用指定处理委托创建后台队列实例。
        /// </summary>
        /// <param name="dealAction">队列元素处理委托。</param>
        public QueueHelper(Action<T> dealAction)
        {
            DealAction = dealAction;
            _worker = Task.Run(ProcessQueue);
        }

        /// <summary>
        /// 将元素加入后台队列。
        /// </summary>
        /// <param name="entity">需要后台处理的元素。</param>
        /// <returns>入队成功返回 <c>true</c>；队列已释放或已停止接收时返回 <c>false</c>。</returns>
        public bool Enqueue(T entity)
        {
            if (_disposed || _queue.IsAddingCompleted)
            {
                return false;
            }

            try
            {
                _queue.Add(entity);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// 停止接收新数据，等待后台消费者处理完已入队数据并释放资源。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _queue.CompleteAdding();

            try
            {
                _worker.GetAwaiter().GetResult();
            }
            finally
            {
                _queue.Dispose();
                _worker.Dispose();
            }
        }

        private void ProcessQueue()
        {
            foreach (T entity in _queue.GetConsumingEnumerable())
            {
                try
                {
                    DealAction?.Invoke(entity);
                }
                catch (Exception ex)
                {
                    LogHelper.Write(ex);
                }
            }
        }
    }
}
