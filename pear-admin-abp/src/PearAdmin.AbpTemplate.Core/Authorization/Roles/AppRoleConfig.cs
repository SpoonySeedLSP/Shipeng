using Abp.MultiTenancy;
using Abp.Zero.Configuration;

namespace PearAdmin.AbpTemplate.Authorization.Roles
{
    /// <summary>
    /// 角色配置
    /// </summary>
    public static class AppRoleConfig
    {
        public static void Configure(IRoleManagementConfig roleManagementConfig)
        {
            // 静态主机角色
            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Host.Admin,
                    MultiTenancySides.Host
                )
            );

            //静态租户的角色
            roleManagementConfig.StaticRoles.Add(
                new StaticRoleDefinition(
                    StaticRoleNames.Tenants.Admin,
                    MultiTenancySides.Tenant
                )
            );
        }
    }
}
