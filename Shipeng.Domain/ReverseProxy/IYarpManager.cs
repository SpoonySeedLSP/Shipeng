using Shipeng.Domain.Shared.Options;

namespace Shipeng.Domain.ReverseProxy
{
    public interface IYarpManager
    {
        /// <summary>
        /// 获取Yarp配置数据
        /// </summary>
        /// <returns></returns>
        Task<YarpOption> GetConfigureAsync();
    }
}
