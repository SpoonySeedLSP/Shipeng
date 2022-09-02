using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.Runtime.Caching;
using PearAdmin.AbpTemplate.Authorization.Roles;
using Abp.Authorization.Roles;
using System.Threading.Tasks;
using Abp.Localization;
using Abp.Zero;

namespace PearAdmin.AbpTemplate.Authorization.Users
{
    public class UserManager : AbpUserManager<Role, User>
    {
        public UserManager(
          RoleManager roleManager,
          UserStore store,
          IOptions<IdentityOptions> optionsAccessor,
          IPasswordHasher<User> passwordHasher,
          IEnumerable<IUserValidator<User>> userValidators,
          IEnumerable<IPasswordValidator<User>> passwordValidators,
          ILookupNormalizer keyNormalizer,
          IdentityErrorDescriber errors,
          IServiceProvider services,
          ILogger<UserManager<User>> logger,
          IPermissionManager permissionManager,
          IUnitOfWorkManager unitOfWorkManager,
          ICacheManager cacheManager,
          IRepository<OrganizationUnit, long> organizationUnitRepository,
          IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
          IOrganizationUnitSettings organizationUnitSettings,
          ISettingManager settingManager,
          IRepository<UserLogin, long> userLoginRepository)
          : base(
              roleManager,
              store,
              optionsAccessor,
              passwordHasher,
              userValidators,
              passwordValidators,
              keyNormalizer,
              errors,
              services,
              logger,
              permissionManager,
              unitOfWorkManager,
              cacheManager,
              organizationUnitRepository,
              userOrganizationUnitRepository,
              organizationUnitSettings,
              settingManager,
              userLoginRepository)
        {
        }

        #region 手机验证码效验方法
        public override async Task<IdentityResult> CreateAsync(User user)
        {
            var result = await CheckDuplicateUsernameOrEmailAddressAsync(user.Id, user.UserName, user.EmailAddress);
            if (!result.Succeeded)
            {
                return result;
            }
            if (string.IsNullOrWhiteSpace(user.EmailAddress))
            {
                user.EmailAddress = string.Empty;
            }


            var tenantId = GetCurrentTenantId();
            if (tenantId.HasValue && !user.TenantId.HasValue)
            {
                user.TenantId = tenantId.Value;
            }

            try
            {
                return await base.CreateAsync(user);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public override async Task<IdentityResult> CheckDuplicateUsernameOrEmailAddressAsync(long? expectedUserId, string userName, string emailAddress)
        {
            var user = (await FindByNameAsync(userName));
            if (user != null && user.Id != expectedUserId)
            {
                //return AbpIdentityResult.Failed(string.Format(L("Identity.DuplicateName"), userName));
            }


            return IdentityResult.Success;
        }

        /// <summary>
        /// 获取当前租户id
        /// </summary>
        /// <returns></returns>
        private int? GetCurrentTenantId()
        {
            //if (_unitOfWorkManager.Current != null)
            //{
            //    return _unitOfWorkManager.Current.GetTenantId();
            //}

            return AbpSession.TenantId;
        }

        private string L(string name)
        {
            return LocalizationManager.GetString(AbpZeroConsts.LocalizationSourceName, name);
        }
        #endregion

    }
}
