using Abp.Auditing;

namespace PearAdmin.AbpTemplate.MultiTenancy.TenantSetting.Dto
{
    /// <summary>
    /// 邮箱设置
    /// </summary>
    public class EmailSettingsEditDto
    {
        /// <summary>
        /// 默认的地址
        /// 没有进行验证，因为我们可能不想使用电子邮件系统
        /// </summary>
        public string DefaultFromAddress { get; set; }

        /// <summary>
        /// 默认从显示名称
        /// </summary>
        public string DefaultFromDisplayName { get; set; }

        /// <summary>
        /// 主机
        /// </summary>
        public string SmtpHost { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string SmtpUserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [DisableAuditing]
        public string SmtpPassword { get; set; }

        /// <summary>
        /// 域
        /// </summary>
        public string SmtpDomain { get; set; }

        /// <summary>
        /// 是否启用启用Ssl
        /// </summary>
        public bool SmtpEnableSsl { get; set; }

        /// <summary>
        /// 是否使用默认凭证
        /// </summary>
        public bool SmtpUseDefaultCredentials { get; set; }
    }
}