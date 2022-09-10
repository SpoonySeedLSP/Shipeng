using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Identity;
using PearAdmin.AbpTemplate.Authorization.Users;
using PearAdmin.AbpTemplate.MultiTenancy;

namespace PearAdmin.AbpTemplate
{
    /// <summary>
    /// 从这个类派生应用程序服务
    /// </summary>
    public abstract class AbpTemplateApplicationServiceBase : ApplicationService
    {
        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }

        protected AbpTemplateApplicationServiceBase()
        {
            LocalizationSourceName = AbpTemplateCoreConsts.LocalizationSourceName;
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("没有当前用户!");
            }

            return user;
        }

        protected virtual Task<Tenant> GetCurrentTenantAsync()
        {
            return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
