using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Roles;
using PearAdmin.AbpTemplate.Authorization.Users;

namespace PearAdmin.AbpTemplate.Authorization.Roles
{
    public class Role : AbpRole<User>
    {
        public const int MaxDescriptionLength = 5000;

        public Role()
        {
        }

        public Role(int? tenantId, string displayName)
            : base(tenantId, displayName)
        {
        }

        public Role(int? tenantId, string name, string displayName)
            : base(tenantId, name, displayName)
        {
        }

        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        public static Role CreateRole(int? tenantId, string name)
        {
            var role = new Role(tenantId, name);
            role.SetNormalizedName();
            return role;
        }

        public Role SetName(string name)
        {
            Name = name;
            DisplayName = name;
            SetNormalizedName();
            return this;
        }

        public Role SetDescription(string description)
        {
            Description = description;
            return this;
        }

        public Role SetIsDefault(bool isDefault)
        {
            IsDefault = isDefault;
            return this;
        }
    }
}
