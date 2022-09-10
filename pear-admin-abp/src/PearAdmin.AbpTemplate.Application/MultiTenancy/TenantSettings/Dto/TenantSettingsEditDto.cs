namespace PearAdmin.AbpTemplate.MultiTenancy.TenantSetting.Dto
{
    public class TenantSettingsEditDto
    {
        /// <summary>
        /// 通用设置
        /// </summary>
        public GeneralSettingsEditDto General { get; set; }

        /// <summary>
        /// 租户邮箱设置
        /// </summary>
        public TenantEmailSettingsEditDto Email { get; set; }

        /// <summary>
        /// 公司设置
        /// </summary>
        public CompanySettingsEditDto CompanySettings { get; set; }

        /// <summary>
        /// 这种验证是针对单租户应用程序进行的
        /// 因为，在单租户应用程序中，这些设置只能由租户设置
        /// </summary>
        public void ValidateHostSettings()
        {
            //var validationErrors = new List<ValidationResult>();
            //if (Clock.SupportsMultipleTimezone && General == null)
            //{
            //    validationErrors.Add(new ValidationResult("General settings can not be null", new[] { "General" }));
            //}

            //if (Email == null)
            //{
            //    validationErrors.Add(new ValidationResult("Email settings can not be null", new[] { "Email" }));
            //}

            //if (validationErrors.Count > 0)
            //{
            //    throw new AbpValidationException("Method arguments are not valid! See ValidationErrors for details.", validationErrors);
            //}
        }
    }
}