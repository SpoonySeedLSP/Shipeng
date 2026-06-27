namespace Shipeng.Util
{
    /// <summary>
    /// 异步转同步辅助类，用于在必须暴露同步 API 的旧调用链中执行异步委托。
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory =
            new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, global::System.Threading.Tasks.TaskScheduler.Default);

        /// <summary>
        /// 以同步方式执行无返回值异步任务，并把异步异常按原始异常重新抛出。
        /// </summary>
        /// <param name="func">需要执行的异步任务工厂。</param>
        public static void RunSync(Func<Task> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            _myTaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 以同步方式执行有返回值异步任务，并返回异步任务结果。
        /// </summary>
        /// <typeparam name="TResult">异步任务返回值类型。</typeparam>
        /// <param name="func">需要执行的异步任务工厂。</param>
        /// <returns>异步任务的执行结果。</returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return _myTaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }
    }
}