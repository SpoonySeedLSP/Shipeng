namespace Shipeng.Application.Contracts.Http
{
    /// <summary>
    /// 简易Http响应异常
    /// </summary>
    public class SimpleHttpException : Exception
    {
        /// <summary>
        /// 简易Http响应异常
        /// </summary>
        /// <param name="message">异常消息</param>
        public SimpleHttpException(string message) : base(message)
        {

        }
    }
}
