using System.Linq;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PearAdmin.AbpTemplate.Authorization;
using PearAdmin.AbpTemplate.Authorization.Roles;
using PearAdmin.AbpTemplate.Authorization.Users;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.Seed.Tenants
{
    /// <summary>
    /// 租户角色和用户构建器
    /// </summary>
    public class TenantRoleAndUserBuilder
    {
        private readonly AbpTemplateDbContext _context;
        private readonly int _tenantId;

        public TenantRoleAndUserBuilder(AbpTemplateDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            CreateRolesAndUsers();
        }

        private void CreateRolesAndUsers()
        {
            // 创建租户管理员角色
            var adminRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Admin);
            if (adminRole == null)
            {
                adminRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin) { IsStatic = true }).Entity;
                _context.SaveChanges();
            }

            // 为租户管理员角色赋予所有权限
            var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == _tenantId && p.RoleId == adminRole.Id)
                .Select(p => p.Name)
                .ToList();

            var permissions = PermissionFinder
                .GetAllPermissions(new AppPermissionProvider())
                .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant) && !grantedPermissions.Contains(p.Name))
                .ToList();

            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = adminRole.Id
                    })
                );
                _context.SaveChanges();
            }

            // 创建租户管理员用户
            var adminUser = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == AbpUserBase.AdminUserName);
            if (adminUser == null)
            {
                //adminUser = User.CreateTenantAdminUser(_tenantId, "admin@defaulttenant.com");
                //adminUser.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(adminUser, "abp@123456");
                //adminUser.IsEmailConfirmed = true;
                //adminUser.IsActive = true;
                adminUser = new User
                {
                    TenantId = _tenantId,
                    //管理员用户名，“admin”不能被删除，“admin用户名”不能被修改
                    UserName = AbpUserBase.AdminUserName,
                    Name = "Admin",
                    Surname = "Tenant",
                    EmailAddress = "admin@defaulttenant.com",
                    IsEmailConfirmed = true,
                    IsActive = true//用户是否活跃，如果用户不活跃，他/她就不能使用该应用程序
                };
                adminUser.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(adminUser, "abp@123456");

                _context.Users.Add(adminUser);
                _context.SaveChanges();

                // 为租户管理员用户分配管理员角色
                _context.UserRoles.Add(new UserRole(_tenantId, adminUser.Id, adminRole.Id));
                _context.SaveChanges();
            }
        }
    }
}
