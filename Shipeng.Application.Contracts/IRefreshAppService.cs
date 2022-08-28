using Shipeng.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace Shipeng.Application.Contracts
{
    public interface IRefreshAppService : IApplicationService
    {

        /// <summary>
        /// 刷新配置数据
        /// </summary>
        /// <param name="refreshConfigure"></param>
        /// <returns></returns>
        Task<ShipengResult> RefreshConfigureAsync(RefreshConfigureDto refreshConfigure);
    }
}
