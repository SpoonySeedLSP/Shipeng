namespace Shipeng.Domain.Middlewares
{
    public interface IMiddlewareContext
    {
        /// <summary>
        /// 执行中间件
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<MiddlewareResult> InvokeMiddlewareAsync();
    }
}
