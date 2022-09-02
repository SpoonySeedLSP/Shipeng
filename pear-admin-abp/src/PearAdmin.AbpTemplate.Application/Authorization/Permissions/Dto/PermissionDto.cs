using Abp.Application.Services.Dto;

namespace PearAdmin.AbpTemplate.Authorization.Permissions.Dto
{
    public class PermissionDto : AuditedEntityDto<long>
    {
        public long? ParentId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }
    }
}
