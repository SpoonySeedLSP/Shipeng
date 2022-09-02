using Abp.Auditing;
using AutoMapper;
using PearAdmin.AbpTemplate.Auditing.Dto;

namespace PearAdmin.AbpTemplate.Monitoring
{
    public class MonitoringMapperProfile : Profile
    {
        public MonitoringMapperProfile()
        {
            CreateMap<AuditLog, AuditLogListDto>();
        }
    }
}
