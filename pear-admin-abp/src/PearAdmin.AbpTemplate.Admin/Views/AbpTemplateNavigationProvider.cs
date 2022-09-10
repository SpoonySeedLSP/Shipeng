using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using PearAdmin.AbpTemplate.Authorization;

namespace PearAdmin.AbpTemplate.Admin.Views
{
    /// <summary>
    /// 菜单定义
    /// </summary>
    public class AbpTemplateNavigationProvider : NavigationProvider
    {
        public override void SetNavigation(INavigationProviderContext context)
        {
            context.Manager.MainMenu
            #region 工作空间
                .AddItem(
                    new MenuItemDefinition(
                        AbpTemplatePageName.WorkSpace,//工作空间
                        L("WorkSpace"),
                        icon: "layui-icon-console",
                        permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_WorkSpace)
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.TenantConsole,
                            L("TenantConsole"),
                            url: "/WorkSpace/TenantConsole",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_WorkSpace_TenantConsole)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.HostConsole,
                            L("HostConsole"),
                            url: "/WorkSpace/HostConsole",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_WorkSpace_HostConsole)
                        )
                    )
                )
            #endregion

            #region 任务中心
                .AddItem(
                    new MenuItemDefinition(
                        AbpTemplatePageName.TaskCenter,//任务中心
                        L("TaskCenter"),
                        icon: "layui-icon-read",
                        permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_TaskCenter)
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.DailyTask,
                            L("DailyTask"),
                            url: "/TaskCenter/DailyTask",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_TaskCenter_DailyTasks)
                        )
                    )
                )
            #endregion

            #region 资源管理
                .AddItem(
                    new MenuItemDefinition(
                        AbpTemplatePageName.ResourceManagement,//资源管理
                        L("ResourceManagement"),
                        icon: "layui-icon-engine",
                        permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_ResourceManagement)
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.DataDictionary,//数据字典
                            L("DataDictionary"),
                            url: "/Resource/DataDictionary",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_ResourceManagement_DataDictionary)
                        )
                    )
                )
            #endregion

            #region 系统管理
                .AddItem(
                    new MenuItemDefinition(
                        AbpTemplatePageName.SystemManagement,//系统管理
                        L("SystemManagement"),
                        icon: "layui-icon-set-fill",
                        permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement)
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.OrganizationUnits,//组织管理
                            L("OrganizationUnitManagement"),
                            url: "OrganizationUnits",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_OrganizationUnits)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.Users,//用户管理
                            L("UserManagement"),
                            url: "Users",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_Users)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.Roles,//角色管理
                            L("RoleManagement"),
                            url: "Roles",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_Roles)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.Permissions,//权限管理
                            L("PermissionManagement"),
                            url: "Permissions",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_Permissions)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.AuditLogs,//审计日志
                            L("AuditLogs"),
                            url: "AuditLogs",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_AuditLogs)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.Editions,//版本管理
                            L("EditionManagement"),
                            url: "Editions",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_Editions)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.Tenants,//租户管理
                            L("TenantManagement"),
                            url: "Tenants",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_Tenants)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.TenantSettings,//租户设置
                            L("TenantSettings"),
                            url: "TenantSettings",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_TenantSettings)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.HostSettings,//宿主设置
                            L("HostSettings"),
                            url: "HostSettings",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_HostSettings)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AbpTemplatePageName.Maintenance,//系统维护
                            L("Maintenance"),
                            url: "Maintenance",
                            icon: "layui-icon-console",
                            permissionDependency: new SimplePermissionDependency(AppPermissionNames.Pages_SystemManagement_Maintenance)
                        )
                    )
                );
            #endregion
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, AbpTemplateCoreConsts.LocalizationSourceName);
        }
    }
}
