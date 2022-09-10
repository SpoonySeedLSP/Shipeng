using AutoMapper;
using PearAdmin.AbpTemplate.Business.Enterprise.Dto;
using PearAdmin.AbpTemplate.MultiTenancy.TenantSetting.Dto;

namespace PearAdmin.AbpTemplate.Business.Enterprise
{
    /// <summary>
    /// 企业模型映射配置文件
    /// </summary>
    public class EnterpriseMapperProfile: Profile
    {
        public EnterpriseMapperProfile()
        {
            //企业信息实体模型转换为企业信息传输模型
            CreateMap<EnterpriseEntity, EnterpriseDto>();
            //企业信息传输模型转换为企业设置修改传输模型
            CreateMap<EnterpriseDto, CompanySettingsEditDto>();
            //企业设置修改传输模型转换为企业信息输入模型
            CreateMap<CompanySettingsEditDto, EnterpriseInput>();
            //企业信息输入模型转换为企业信息实体模型
            CreateMap<EnterpriseInput, EnterpriseEntity>();
        }
    }
}
