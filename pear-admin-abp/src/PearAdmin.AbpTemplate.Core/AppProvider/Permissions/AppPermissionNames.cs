namespace PearAdmin.AbpTemplate.Authorization
{
    public static class AppPermissionNames
    {
        #region GlobalPermission
        public const string Pages = "Pages";
        #endregion

        #region SystemManagement
        public const string Pages_SystemManagement = "系统管理";

        //组织
        public const string Pages_SystemManagement_OrganizationUnits = "部门管理";
        public const string Pages_SystemManagement_OrganizationUnits_Create = "添加";
        public const string Pages_SystemManagement_OrganizationUnits_Update = "修改";
        public const string Pages_SystemManagement_OrganizationUnits_Delete = "删除";
        public const string Pages_SystemManagement_OrganizationUnits_MoveOrganizationUnit = "移动";

        //用户
        public const string Pages_SystemManagement_Users = "用户管理";
        public const string Pages_SystemManagement_Users_Create = "添加用户";
        public const string Pages_SystemManagement_Users_Update = "修改用户信息";
        public const string Pages_SystemManagement_Users_Delete = "删除用户";
        public const string Pages_SystemManagement_Users_ResetPassword = "重置用户密码";

        //角色
        public const string Pages_SystemManagement_Roles = "角色管理";
        public const string Pages_SystemManagement_Roles_Create = "添加角色";
        public const string Pages_SystemManagement_Roles_Update = "修改角色";
        public const string Pages_SystemManagement_Roles_Delete = "删除角色";

        //权限
        public const string Pages_SystemManagement_Permissions = "权限管理";

        public const string Pages_SystemManagement_AuditLogs = "审计日志";

        public const string Pages_SystemManagement_Editions = "版本";
        public const string Pages_SystemManagement_Editions_Create = "添加版本";
        public const string Pages_SystemManagement_Editions_Update = "修改版本";
        public const string Pages_SystemManagement_Editions_Delete = "删除版本";

        public const string Pages_SystemManagement_Tenants = "租户管理";
        public const string Pages_SystemManagement_Tenants_ChangeTenantEdition = "修改租户版本";

        public const string Pages_SystemManagement_TenantSettings = "租户设置";
        public const string Pages_SystemManagement_HostSettings = "主机设置";

        public const string Pages_SystemManagement_Maintenance = "Pages.SystemManagement.Maintenance";
        public const string Pages_SystemManagement_Maintenance_Logs = "Pages.SystemManagement.Maintenance.Logs";
        public const string Pages_SystemManagement_Maintenance_Logs_DownLoad = "Pages.SystemManagement.Maintenance.Logs.DownLoad";
        public const string Pages_SystemManagement_Maintenance_Logs_Refresh = "Pages.SystemManagement.Maintenance.Logs.Refresh";

        public const string Pages_SystemManagement_HangfireDashboard = "Pages.SystemManagement.HangfireDashboard";
        public const string Pages_SystemManagement_LogDashboard = "Pages.SystemManagement.LogDashboard";
        #endregion

        #region ResourceManagement
        public const string Pages_ResourceManagement = "Pages.ResourceManagement";

        public const string Pages_ResourceManagement_DataDictionary = "Pages.ResourceManagement.DataDictionary";
        public const string Pages_ResourceManagement_DataDictionary_DataDictionaryItem = "Pages.ResourceManagement.DataDictionary.DataDictionaryItem";
        public const string Pages_ResourceManagement_DataDictionary_DataDictionaryItem_Create = "Pages.ResourceManagement.DataDictionary.DataDictionaryItem.Create";
        public const string Pages_ResourceManagement_DataDictionary_DataDictionaryItem_Update = "Pages.ResourceManagement.DataDictionary.DataDictionaryItem.Update";
        public const string Pages_ResourceManagement_DataDictionary_DataDictionaryItem_Delete = "Pages.ResourceManagement.DataDictionary.DataDictionaryItem.Delete";
        #endregion

        #region TaskCenter
        public const string Pages_TaskCenter = "Pages.TaskCenter";

        public const string Pages_TaskCenter_DailyTasks = "Pages.TaskCenter.DailyTasks";
        public const string Pages_TaskCenter_DailyTasks_Create = "Pages.TaskCenter.DailyTasks.Create";
        public const string Pages_TaskCenter_DailyTasks_Update = "Pages.TaskCenter.DailyTasks.Update";
        public const string Pages_TaskCenter_DailyTasks_Delete = "Pages.TaskCenter.DailyTasks.Delete";
        #endregion

        #region WorkSpace
        public const string Pages_WorkSpace = "Pages.WorkSpace";
        public const string Pages_WorkSpace_TenantConsole = "Pages.WorkSpace.TenantConsole";
        public const string Pages_WorkSpace_HostConsole = "Pages.WorkSpace.HostConsole";
        #endregion
    }
}
