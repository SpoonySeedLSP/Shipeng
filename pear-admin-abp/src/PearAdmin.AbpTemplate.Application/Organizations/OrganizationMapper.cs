using Abp.Organizations;
using AutoMapper;
using PearAdmin.AbpTemplate.Organizations.Dto;

namespace PearAdmin.AbpTemplate.Organizations
{
    public class OrganizationMapperProfile : Profile
    {
        public OrganizationMapperProfile()
        {
            CreateMap<OrganizationUnit, OrganizationUnitDto>();
        }
    }
}
