using Shipeng.Domain.Entities;

namespace Shipeng.Domain.Authorization
{
    public interface IAuthenticationManager
    {
        /// <summary>
        /// 获取身份认证配置项信息
        /// </summary>
        /// <returns></returns>
        Task<AuthenticationConfigure> GetConfigureAsync();
    }
}
