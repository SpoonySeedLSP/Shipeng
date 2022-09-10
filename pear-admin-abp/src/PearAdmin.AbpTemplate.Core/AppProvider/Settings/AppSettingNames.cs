namespace PearAdmin.AbpTemplate.Settings
{
    /// <summary>
    /// 基础配置定义
    /// </summary>
    public static class AppSettingNames
    {
        /// <summary>
        /// 主机管理
        /// </summary>
        public static class HostManagement
        {
            public const string CompanyName = "App.HostManagement.CompanyName";
            public const string CompanyAddress = "App.HostManagement.CompanyAddress";
        }

        /// <summary>
        /// 租户管理
        /// </summary>
        public static class TenantManagement
        {
            //默认的版本
            public const string DefaultEdition = "App.TenantManagement.DefaultEdition";
            //公司名称
            public const string CompanyName = "App.TenantManagement.CompanyName";
            //公司地址
            public const string CompanyAddress = "App.TenantManagement.CompanyAddress";
            //邀请邮箱模板
            public const string InviteMailboxTemplate = "App.TenantManagement.InviteMailboxTemplate";
        }

        public static class Email
        {
            //使用主机默认电子邮件设置
            public const string UseHostDefaultEmailSettings = "App.Email.UseHostDefaultEmailSettings";
        }
    }
}
