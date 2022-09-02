using Abp.Authorization;

namespace PearAdmin.AbpTemplate.Admin.Filter
{
    /// <summary>
    /// 注意：关于IPermissionChecker接口
    /// Abp权限系统使用IPermissionChecker去检查授权
    /// 同时你可以根据需要实现你自己的方式
    /// 如果IPermissionChecker没有被实现，NullPermissionChecker会被使用于授权所有权限给每个人
    /// 定义权限
    /// 在使用验证权限前，我们需要为每一个操作定义唯一的权限
    /// Abp的设计是基于模块化，所以不同的模块可以有不同的权限
    /// 为了定义权限，一个模块应该创建AuthorizationProvider的派生类
    /// AbpAuthorizationProvider继承自AuthorizationProvider，换句话说就是AuthorizationProvider派生出AbpAuthorizationProvider
    public class AbpAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            //创建权限
            var administration = context.CreatePermission("Administration");

            var userManagement = administration.CreateChildPermission("Administration.UserManagement");
            userManagement.CreateChildPermission("Administration.UserManagement.CreateUser");

            var roleManagement = administration.CreateChildPermission("Administration.RoleManagement");
        }
    }

}
