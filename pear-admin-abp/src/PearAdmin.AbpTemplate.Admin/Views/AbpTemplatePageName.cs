namespace PearAdmin.AbpTemplate.Admin.Views
{
    /// <summary>
    /// Abp模板页名称
    /// </summary>
    public class AbpTemplatePageName
    {
        #region 主页 Home
        public const string Home = "Home";
        #endregion

        #region 系统管理 SystemManagement
        public const string SystemManagement = "SystemManagement";
        public const string OrganizationUnits = "OrganizationUnits";//部门管理
        public const string Users = "Users";//用户管理
        public const string Roles = "Roles";//角色管理
        public const string Permissions = "Permissions";//权限管理
        public const string AuditLogs = "AuditLogs";//审计日志
        public const string Editions = "Editions";//版本
        public const string Tenants = "Tenants";//租户
        public const string TenantSettings = "TenantSettings";//租户管理
        public const string HostSettings = "HostSettings";//宿主设置
        public const string Maintenance = "Maintenance";//
        #endregion

        #region 资源管理 ResourceManagement
        public const string ResourceManagement = "ResourceManagement";//资源管理
        public const string DataDictionary = "DataDictionary";//数据字典
        #endregion

        #region 任务中心 TaskCenter
        public const string TaskCenter = "TaskCenter";
        public const string DailyTask = "DailyTask";
        #endregion

        #region 工作空间 WorkSpace
        public const string WorkSpace = "WorkSpace";
        public const string TenantConsole = "TenantConsole";
        public const string HostConsole = "HostConsole";
        #endregion
    }
}
