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

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.Seed.Host
{
    /// <summary>
    /// 宿主角色和用户创建者
    /// </summary>
    public class HostRoleAndUserCreator
    {
        private readonly AbpTemplateDbContext _context;

        public HostRoleAndUserCreator(AbpTemplateDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateHostRoleAndUsers();
        }

        private void CreateHostRoleAndUsers()
        {
            // 为宿主创建管理员角色
            var adminRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.Admin);
            if (adminRoleForHost == null)
            {
                adminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.Admin, StaticRoleNames.Host.Admin)
                {
                    IsStatic = true,
                    IsDefault = true
                }).Entity;
                _context.SaveChanges();
            }

            // 为宿主管理员角色分配宿主所有权限
            var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == null && p.RoleId == adminRoleForHost.Id)
                .Select(p => p.Name)
                .ToList();

            var permissions = PermissionFinder
                .GetAllPermissions(new AppPermissionProvider())
                .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Host) && !grantedPermissions.Contains(p.Name))
                .ToList();

            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = null,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = adminRoleForHost.Id
                    })
                );
                _context.SaveChanges();
            }

            // 为宿主创建管理员用户
            var adminUserForHost = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == null && u.UserName == AbpUserBase.AdminUserName);
            if (adminUserForHost == null)
            {
                var user = new User
                {
                    TenantId = null,
                    //管理员用户名，“admin”不能被删除，“admin用户名”不能被修改
                    UserName = AbpUserBase.AdminUserName,
                    Name = "Admin",
                    Surname = "Host",
                    EmailAddress = "admin@host.com",
                    IsEmailConfirmed = true,
                    IsActive = true//用户是否活跃，如果用户不活跃，他/她就不能使用该应用程序
                };

                user.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(user, "abp@123456");
                user.SetNormalizedNames();

                adminUserForHost = _context.Users.Add(user).Entity;
                _context.SaveChanges();

                // 为宿主管理员用户分配宿主管理员角色
                _context.UserRoles.Add(new UserRole(null, adminUserForHost.Id, adminRoleForHost.Id));
                _context.SaveChanges();
            }
        }
    }
}
