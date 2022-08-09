using Shipeng.Application.Contracts.Dtos;
using Shipeng.Application.Contracts.Dtos.Node;
using Volo.Abp.Application.Services;

namespace Shipeng.Application.Contracts
{
    public interface IConfigureAppService : IApplicationService
    {
        /// <summary>
        /// 根据推送配置获取配置项数据
        /// </summary>
        /// <param name="reloadConfigure"></param>
        /// <returns></returns>
        Task<ShipengResult<RefreshConfigureDto>> GetConfigureAsync(ReloadConfigureDto reloadConfigure);
    }
}
