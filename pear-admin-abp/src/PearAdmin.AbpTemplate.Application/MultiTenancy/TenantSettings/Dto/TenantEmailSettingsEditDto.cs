namespace PearAdmin.AbpTemplate.MultiTenancy.TenantSetting.Dto
{
    /// <summary>
    /// 租户邮箱设置
    /// </summary>
    public class TenantEmailSettingsEditDto : EmailSettingsEditDto
    {
        /// <summary>
        /// 是否使用主机默认电子邮件设置
        /// </summary>
        public bool UseHostDefaultEmailSettings { get; set; }
    }
}