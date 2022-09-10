using Abp.Application.Services;
using PearAdmin.AbpTemplate.Business.Enterprise.Dto;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Business.Enterprise
{
    /// <summary>
    /// 企业服务
    /// </summary>
    public interface IEnterpriseService: IApplicationService
    {
        /// <summary>
        /// 根据租户id和用户id获取企业信息
        /// </summary>
        /// <param name="tenantId">租户id</param>
        /// <param name="userId">用户id</param>
        /// <returns></returns>
        Task<EnterpriseDto> GetEnterpriseInfoAsync(int tenantId,long userId);

        /// <summary>
        /// 保存企业信息
        /// </summary>
        /// <param name="input">企业信息输入模型</param>
        /// <returns></returns>
        Task SaveEnterpriseInfoAsync(EnterpriseInput input);

    }
}
