using Abp.Auditing;
using PearAdmin.AbpTemplate.Authorization.Users;
using PearAdmin.AbpTemplate.MultiTenancy;
using PearAdmin.AbpTemplate.Security;
using PearAdmin.AbpTemplate.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PearAdmin.AbpTemplate.Admin.Models.Account
{
    /// <summary>
    /// 手机注册模型
    /// </summary>
    public class MobileRegisterViewModel : IValidatableObject
    {
        [StringLength(Tenant.MaxTenancyNameLength)]
        public string TenancyName { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        [Required]
        [StringLength(User.MaxMobilePhoneLength)]
        public string MobilePhone { get; set; }

        /// <summary>
        /// 手机验证码
        /// </summary>
        [Required]
        [StringLength(User.MobileCodeLength)]
        public string MobilePhoneCode { get; set; }

        [StringLength(User.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }
        public PasswordComplexitySetting PasswordComplexitySetting { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(MobilePhone))
            {
                if (ValidationHelper.IsPhoneNumber(MobilePhone))
                {
                    yield return new ValidationResult("您输入的手机号码无效 !");
                }
            }
            else
            {
                yield return new ValidationResult("手机号码不能为空 !");
            }
        }
    }
}
