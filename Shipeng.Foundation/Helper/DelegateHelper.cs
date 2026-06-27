namespace Shipeng.Util
{
    /// <summary>
    /// 委托帮助类
    /// </summary>
    public class DelegateHelper
    {
        /// <summary>
        /// 异步执行方法
        /// </summary>
        /// <param name="firstFunc"> 首先执行的方法 </param>
        /// <param name="next"> 接下来执行的方法 </param>
        public static void RunAsync(Action firstFunc, Action next)
        {
            Task.Run(firstFunc).ContinueWith(task =>
            {
                next();
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 异步执行方法
        /// </summary>
        /// <param name="firstFunc"> 首先执行的方法 </param>
        /// <param name="next"> 接下来执行的方法 </param>
        public static void RunAsync(Func<object> firstFunc, Action<object> next)
        {
            Task.Run(firstFunc).ContinueWith(task =>
            {
                next(task.Result);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
