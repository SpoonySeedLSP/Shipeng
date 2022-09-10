using Abp.MultiTenancy;

namespace PearAdmin.AbpTemplate.Admin.Models.Account
{
    public class LoginFormViewModel
    {
        public string ReturnUrl { get; set; }

        /// <summary>
        /// 是否启用多租户
        /// </summary>
        public bool IsMultiTenancyEnabled { get; set; }

        /// <summary>
        /// 是否已启用宿主注册
        /// </summary>
        public bool IsSelfRegistrationAllowed { get; set; }

        public MultiTenancySides MultiTenancySide { get; set; }
    }
}
