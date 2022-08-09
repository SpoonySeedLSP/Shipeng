using Shipeng.Application.Contracts;
using Shipeng.Application.Contracts.Http;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace Shipeng.Application
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseApplicationService : ApplicationService
    {
        /// <summary>
        /// 抛出异常
        /// </summary>
        /// <param name="msg">消息</param>
        /// <exception cref="Exception"></exception>
        [RemoteService(IsEnabled = false)]
        public static void ThrownFailed(string msg)
        {
            throw new SimpleHttpException(msg);
        }
        /// <summary>
        /// 成功返回
        /// </summary>
        /// <returns></returns>
        [RemoteService(IsEnabled = false)]
        public static ShipengResult Ok()
        {
            return new ShipengResult()
            {
                Code = 0,
                Message = "success"
            };
        }
        /// <summary>
        /// 成功返回
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        [RemoteService(IsEnabled = false)]
        public static ShipengResult<TResult> Ok<TResult>(TResult data)
        {
            return new ShipengResult<TResult>()
            {
                Code = 0,
                Message = "success",
                Data = data
            };
        }
        /// <summary>
        /// 成功返回
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="totalCount">总记录数</param>
        /// <returns></returns>
        [RemoteService(IsEnabled = false)]
        public static ShipengPageResult<TResult> Ok<TResult>(TResult data, int totalCount)
        {
            return new ShipengPageResult<TResult>()
            {
                Code = 0,
                Message = "success",
                Data = data,
                Count = totalCount
            };
        }
        /// <summary>
        /// 返回自定义状态码 
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        [RemoteService(IsEnabled = false)]
        public static ShipengResult Customize(int code, string message = "")
        {
            return new ShipengResult()
            {
                Code = code,
                Message = message
            };
        }
        /// <summary>
        /// 返回自定义状态码 
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [RemoteService(IsEnabled = false)]
        public static ShipengResult<TResult> Customize<TResult>(int code, string message, TResult data)
        {
            return new ShipengResult<TResult>()
            {
                Code = code,
                Message = message,
                Data = data
            };
        }
    }
}
