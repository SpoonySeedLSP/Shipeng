using System.Collections.Concurrent;

namespace Shipeng.Util
{
    /// <summary>
    /// 任务队列。
    /// </summary>
    public sealed class TaskQueue : IDisposable
    {
        #region 构造函数

        /// <summary>
        /// 默认队列，任务之间不额外等待。
        /// </summary>
        public TaskQueue()
        {
            _timeSpan = TimeSpan.Zero;
            Start();
        }

        /// <summary>
        /// 创建带执行间隔的任务队列。
        /// </summary>
        /// <param name="timeSpan">每个任务执行完成后的等待时间；传入 <see cref="TimeSpan.Zero"/> 表示不等待。</param>
        public TaskQueue(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
            Start();
        }

        #endregion 构造函数

        #region 私有成员

        /// <summary>
        /// 待执行任务集合。
        /// </summary>
        /// <remarks>
        /// BlockingCollection 在队列为空时会阻塞消费者线程，避免手动组合 Semaphore 与 ConcurrentQueue 时
        /// 容易出现的信号丢失、线程空转和停止退出不完整问题。
        /// </remarks>
        private BlockingCollection<Action> _taskList { get; } = new BlockingCollection<Action>(new ConcurrentQueue<Action>());

        private CancellationTokenSource _cancellationTokenSource { get; } = new CancellationTokenSource();

        private Task _worker { get; set; }

        private void Start()
        {
            _worker = Task.Factory.StartNew(async () =>
            {
                foreach (Action task in _taskList.GetConsumingEnumerable(_cancellationTokenSource.Token))
                {
                    try
                    {
                        task?.Invoke();

                        if (_timeSpan != TimeSpan.Zero)
                        {
                            await Task.Delay(_timeSpan, _cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        HandleException?.Invoke(ex);
                    }
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, global::System.Threading.Tasks.TaskScheduler.Default).Unwrap();
        }

        private TimeSpan _timeSpan { get; set; }

        #endregion 私有成员

        #region 外部接口

        /// <summary>
        /// 停止队列接收新任务，并通知后台消费者尽快退出。
        /// </summary>
        public void Stop()
        {
            if (!_taskList.IsAddingCompleted)
            {
                _taskList.CompleteAdding();
            }

            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// 添加一个待执行任务。
        /// </summary>
        /// <param name="task">需要在队列后台线程中执行的委托。</param>
        public void Enqueue(Action task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            _taskList.Add(task, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// 获取当前仍在等待执行的任务数量。
        /// </summary>
        /// <returns>尚未被后台消费者取出的任务数。</returns>
        public int GetActionCount()
        {
            return _taskList.Count;
        }

        /// <summary>
        /// 后台任务执行出现异常时触发，便于调用方集中记录日志或告警。
        /// </summary>
        public Action<Exception> HandleException { get; set; }

        /// <summary>
        /// 释放队列使用的取消令牌、阻塞集合和后台任务资源。
        /// </summary>
        public void Dispose()
        {
            Stop();
            try
            {
                _worker?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException))
            {
            }
            finally
            {
                _taskList.Dispose();
                _cancellationTokenSource.Dispose();
            }
        }

        #endregion 外部接口
    }
}