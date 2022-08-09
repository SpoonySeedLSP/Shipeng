namespace Shipeng.Domain.ReverseProxy
{
    public interface IRefreshManager
    {
        /// <summary>
        /// 重载网关配置
        /// </summary>
        /// <returns></returns>
        Task ReloadConfigAsync();
    }
}
