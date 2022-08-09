using Shipeng.Application.Contracts.Dtos.Authorization;
using Volo.Abp.Application.Services;

namespace Shipeng.Application.Contracts
{
    /// <summary>
    /// 授权相关接口
    /// </summary>
    public interface IAuthorizationAppService:IApplicationService
    {
        /// <summary>
        /// 获取身份认证配置信息
        /// </summary>
        /// <returns></returns>
        Task<ShipengResult<SaveAuthenticationDto>> GetAuthenticationAsync();
        
        /// <summary>
        /// 保存身份认证配置信息
        /// </summary>
        /// <param name="authenticationDto"></param>
        /// <returns></returns>
        Task<ShipengResult> SaveAuthenticationAsync(SaveAuthenticationDto authenticationDto);
    }
}
