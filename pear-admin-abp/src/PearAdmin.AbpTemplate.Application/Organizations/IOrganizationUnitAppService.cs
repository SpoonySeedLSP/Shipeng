using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using PearAdmin.AbpTemplate.Organizations.Dto;

namespace PearAdmin.AbpTemplate.Organizations
{
    /// <summary>
    /// 组织机构应用服务
    /// </summary>
    public interface IOrganizationUnitAppService : IApplicationService
    {
        /// <summary>
        /// 获取组织机构（树形结构数据）
        /// </summary>
        /// <returns></returns>
        Task<ListResultDto<OrganizationUnitDto>> GetAllOrganizationUnitTree();

        /// <summary>
        /// 获取子级组织机构及每个组织机构下的用户数量（树形结构数据）
        /// </summary>
        /// <param name="input">父级组织机构</param>
        /// <returns></returns>
        Task<PagedResultDto<OrganizationUnitDto>> GetPagedOrganizationUnit(GetPagedOrganizationUnitInput input);

        /// <summary>
        /// 获取当前用户所属组织
        /// </summary>
        /// <param name="userId">当前登录用户id</param>
        /// <returns></returns>
        Task<List<OrganizationUnitDto>> GetOrganizationUnitListByUserIdAsync(long userId);

        /// <summary>
        /// 根据组织id集合获取组织信息数据
        /// </summary>
        /// <param name="ids">组织id集合</param>
        /// <returns></returns>
        Task<List<OrganizationUnitDto>> GetOrganizationUnitListByIdsAsync(List<long> ids);

        /// <summary>
        /// 获取组织机构编辑
        /// </summary>
        /// <returns></returns>
        Task<OrganizationUnitDto> GetOrganizationUnitForEdit(NullableIdDto<long> input);

        /// <summary>
        /// 创建组织机构
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task CreateOrganizationUnit(CreateOrganizationUnitDto input);

        /// <summary>
        /// 更新组织机构
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateOrganizationUnit(UpdateOrganizationUnitDto input);

        /// <summary>
        /// 删除组织机构
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteOrganizationUnit(List<EntityDto<long>> inputs);

        /// <summary>
        /// 将目标组织机构移入到指定组织机构
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<OrganizationUnitDto> MoveOrganizationUnit(MoveOrganizationUnitInput input);
    }
}
