using Abp;
using Abp.Domain.Repositories;
using PearAdmin.AbpTemplate.Business.Enterprise.Dto;
using System;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Business.Enterprise
{
    /// <summary>
    /// 企业服务的实现
    /// </summary>
    public class EnterpriseService : AbpTemplateApplicationServiceBase, IEnterpriseService
    {
        private readonly IRepository<EnterpriseEntity, long> _enterpriseRepository;

        public EnterpriseService(IRepository<EnterpriseEntity, long> enterpriseRepository)
        {
            _enterpriseRepository = enterpriseRepository;
        }

        /// <summary>
        /// 根据租户id和用户id获取企业信息
        /// </summary>
        /// <param name="tenantId">租户id</param>
        /// <param name="userId">用户id</param>
        /// <returns></returns>
        public async Task<EnterpriseDto> GetEnterpriseInfoAsync(int tenantId, long userId)
        {
            var enterpriseInfo =await _enterpriseRepository
                .FirstOrDefaultAsync(r=>r.TenantId==tenantId && r.UserId==userId);
            if (enterpriseInfo == null) return null;
            var result = ObjectMapper.Map<EnterpriseDto>(enterpriseInfo);
            return result;
        }

        /// <summary>
        /// 保存企业信息
        /// </summary>
        /// <param name="input">企业信息输入模型</param>
        /// <returns></returns>
        public async Task SaveEnterpriseInfoAsync(EnterpriseInput input)
        {
            try
            {
                var entity = await _enterpriseRepository
                .FirstOrDefaultAsync(r => r.TenantId == input.TenantId && r.UserId == input.UserId);
                long id = entity?.Id ?? 0;
                if (entity == null)
                {
                    var addEntity = ObjectMapper.Map<EnterpriseEntity>(input);
                    addEntity.CreationTime = DateTime.Now;
                    addEntity.UpdateTime = DateTime.Now;
                    //插入一个新实体并获取它的Id,它可能需要保存当前工作单元才能检索id
                    id = await _enterpriseRepository.InsertAndGetIdAsync(addEntity);
                }
                else
                {
                    var updateEntity= ObjectMapper.Map(input, entity);
                    updateEntity.UpdateTime = DateTime.Now;
                    entity = await _enterpriseRepository.UpdateAsync(updateEntity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new AbpException($"保存企业信息发生异常：{ex.Message}");
            }
        }

    }
}
