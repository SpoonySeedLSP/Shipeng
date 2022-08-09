using Shipeng.Application.Contracts.Dtos.ServiceGovernance;

namespace Shipeng.Application.Contracts
{
    /// <summary>
    /// 服务治理相关配置
    /// </summary>
    public interface IServiceGovernanceAppService
    {
        /// <summary>
        /// 获取服务治理配置信息
        /// </summary>
        /// <returns></returns>
        Task<ShipengResult<ServiceGovernanceConfigureDto>> GetServiceGovernanceConfigureAsync();
        /// <summary>
        /// 保存服务治理相关配置信息
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        Task<ShipengResult> SaveServiceGovernanceConfigureAsync(ServiceGovernanceConfigureDto configure);
    }
}
