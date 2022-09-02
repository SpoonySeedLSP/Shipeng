using Abp.Application.Editions;
using AutoMapper;
using PearAdmin.AbpTemplate.MultiTenancy.Editions.Dto;
using PearAdmin.AbpTemplate.MultiTenancy.Tenants.Dto;

namespace PearAdmin.AbpTemplate.MultiTenancy
{
    public class MonitoringMapperProfile : Profile
    {
        public MonitoringMapperProfile()
        {
            CreateMap<Tenant, TenantDto>();
            CreateMap<Edition, EditionDto>();
        }
    }
}
