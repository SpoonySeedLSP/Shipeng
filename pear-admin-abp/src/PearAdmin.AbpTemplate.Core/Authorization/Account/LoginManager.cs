using Microsoft.AspNetCore.Identity;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Zero.Configuration;
using PearAdmin.AbpTemplate.Authorization.Roles;
using PearAdmin.AbpTemplate.Authorization.Users;
using PearAdmin.AbpTemplate.MultiTenancy;
using System.Threading.Tasks;
using Abp.Extensions;
using System;
using PearAdmin.AbpTemplate.Core.Authorization;
using Abp.Timing;
using Abp.UI;

namespace PearAdmin.AbpTemplate.Authorization
{
    public class LogInManager : AbpLogInManager<Tenant, Role, User>
    {
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly AbpUserManager<Role, User> _userManager;
        private readonly UserStore _userStore;

        public LogInManager(
            UserManager userManager, 
            IMultiTenancyConfig multiTenancyConfig,
            IRepository<Tenant> tenantRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager, 
            IRepository<UserLoginAttempt, long> userLoginAttemptRepository, 
            IUserManagementConfig userManagementConfig,
            IIocResolver iocResolver,
            IPasswordHasher<User> passwordHasher, 
            RoleManager roleManager,
            UserClaimsPrincipalFactory claimsPrincipalFactory,
            UserStore userStore) 
            : base(
                  userManager, 
                  multiTenancyConfig,
                  tenantRepository, 
                  unitOfWorkManager, 
                  settingManager, 
                  userLoginAttemptRepository, 
                  userManagementConfig, 
                  iocResolver, 
                  passwordHasher, 
                  roleManager, 
                  claimsPrincipalFactory)
        {
            _multiTenancyConfig = multiTenancyConfig;
            _unitOfWorkManager = unitOfWorkManager;
            _tenantRepository = tenantRepository;
            _userManager = userManager;
            _userStore = userStore;
        }

        /// <summary>
        /// 自定义登录
        /// </summary>
        /// <param name="account">账号、手机号、身份证号</param>
        /// <param name="password">明文密码</param>
        /// <returns></returns>
        [UnitOfWork]
        public virtual async Task<AbpLoginResult<Tenant, User>> LoginCustomAsync(string account, string password)
        {
            var result = await LoginCustomAsyncInternal(account, password);

            //保存用户尝试登录的记录
            await SaveLoginAttemptAsync(result, null, account);
            return result;
        }

        protected virtual async Task<AbpLoginResult<Tenant, User>> LoginCustomAsyncInternal(string account, string password)
        {
            if (account.IsNullOrEmpty() || password.IsNullOrEmpty())
            {
                throw new ArgumentException("帐号或密码为空");
            }

            //不启用租户，获取默认租户
            Tenant tenant = await GetDefaultTenantAsync();

            int? tenantId = tenant?.Id;
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                //根据用户名获取用户信息
                var user = await _userStore.FindByAccountAsync(account);
                if (user == null)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.UnknownExternalLogin, tenant);
                }

                //验证用户的密码是否正确
                var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.Password, password);
                if (verificationResult != PasswordVerificationResult.Success)
                {
                    if (await TryLockOutAsync(tenantId, user.Id))
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                    }

                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidPassword, tenant, user);
                }

                //重置用户登录失败次数
                await _userManager.ResetAccessFailedCountAsync(user);

                //生成登录结果
                return await CreateLoginResultAsync(user, tenant);
            }
        }

        #region 通过手机异步登录
        /// <summary>
        /// 通过手机异步登录
        /// </summary>
        /// <param name="mobilePhone">手机号</param>
        /// <param name="plainPassword">普通密码</param>
        /// <param name="tenancyName ">租户名</param>
        /// <param name="shouldLockout">是否锁定</param>
        /// <returns></returns>
        public virtual async Task<AbpLoginResult<Tenant, User>> LoginByMobileAsync(string mobilePhone, string plainPassword,
            string tenancyName = null,bool shouldLockout = true)
        {
            var result = await LoginByMobileAsyncInternal(mobilePhone, plainPassword, tenancyName, shouldLockout);
            await SaveLoginAttemptAsync(result, tenancyName, mobilePhone);
            return result;
        }

        /// <summary>
        /// 手机验登陆内部方法
        /// </summary>
        /// <param name="mobilePhone">手机号</param>
        /// <param name="plainPassword">普通密码</param>
        /// <param name="tenancyName">租户名</param>
        /// <param name="shouldLockout">是否锁定</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected async Task<AbpLoginResult<Tenant, User>> LoginByMobileAsyncInternal(string mobilePhone, string plainPassword, string tenancyName, bool shouldLockout)
        {
            if (mobilePhone.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(mobilePhone));
            }

            if (plainPassword.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(plainPassword));
            }

            //获取并检查租户
            Tenant tenant = null;
            //设置当前租户id为空
            using (_unitOfWorkManager.Current.SetTenantId(null))
            {
                //如果关闭租户就获取默认租户
                if (!_multiTenancyConfig.IsEnabled)
                {
                    //获取默认租户
                    tenant = await GetDefaultTenantAsync();
                }
                //租户id是否为空
                else if (!string.IsNullOrWhiteSpace(tenancyName))
                {
                    //查询租户信息
                    tenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                    if (tenant == null)
                    {
                        //返回无效租户
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidTenancyName);
                    }
                    if (!tenant.IsActive)
                    {
                        //返回租户未启用
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.TenantIsNotActive, tenant);
                    }
                }
            }
            //设置租户id
            var tenantId = tenant == null ? (int?)null : tenant.Id;
            //设置当前租户id
            using (_unitOfWorkManager.Current.SetTenantId(tenantId))
            {
                //初始化选项
                await _userManager.InitializeOptionsAsync(tenantId);
                var loggedInFromExternalSource = false;
                //通过手机号查询用户
                var user = await _userStore.FindByPhoneNumberAsync(tenantId, mobilePhone);
                if (user == null)
                    //返回无效用户
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);
                //用户是否被锁定
                if (await _userManager.IsLockedOutAsync(user))
                    //返回锁定用户
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                if (!loggedInFromExternalSource)
                {
                    /* 返回为指定用户提供的密码的散列表示形式
                     * user:要对其密码进行散列处理的用户
                     * password:要散列的密码
                     * 返回结果:为指定用户提供的密码的散列表示形式
                     */
                    string hashedPassword = _userManager.PasswordHasher.HashPassword(user, plainPassword);
                    /* 这个方法的实现应该是时间一致的
                     * user:需要验证其密码的用户
                     * hashedPassword:散列密码
                     * providedPassword:提供的密码
                     * 返回结果:一个Microsoft.AspNetCore.Identity.PasswordVerificationResult，表示密码散列比较的结果
                     */
                    var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user,user.Password, plainPassword);
                    if (verificationResult != PasswordVerificationResult.Success)
                    {
                        //是否锁定
                        if (shouldLockout)
                        {
                            //锁定账户
                            if (await TryLockOutAsync(tenantId, user.Id))
                                //返回锁定账户
                                return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                        }
                        //返回无效密码
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidPassword, tenant, user);
                    }
                    //重置登陆失败次数
                    await _userManager.ResetAccessFailedCountAsync(user);
                }
                return await CreateLoginByMobileResultAsync(user, tenant);
            }
        }

        /// <summary>
        /// 创建手机号登陆结果
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="tenant">租户</param>
        /// <returns></returns>
        protected async Task<AbpLoginResult<Tenant, User>> CreateLoginByMobileResultAsync(User user, Tenant tenant = null)
        {
            //用户未激活
            if (!user.IsActive)
                //返回用户未激活
                return new AbpLoginResult<Tenant, User>(AbpLoginResultType.UserIsNotActive, tenant, user);
            //用户手机号未认证
            if (!user.IsPhoneNumberConfirmed)
                //返回用户手机号未认证
                //return new AbpLoginResult<Tenant, User>(AbpLoginResultType.UserPhoneNumberIsNotConfirmed, tenant, user);
                return new AbpLoginResult<Tenant, User>((AbpLoginResultType)MobileAbpLoginResultType.UserMobilePhoneNotConfirmed);
            user.LastLoginTime = Clock.Now;
            //更新用户
            await _userManager.UpdateAsync(user);
            await _unitOfWorkManager.Current.SaveChangesAsync();
            var identityResult = await _userManager.CreateAsync(user);
            if(!identityResult.Succeeded) throw new UserFriendlyException("登录失败");
            return new AbpLoginResult<Tenant, User>(
                AbpLoginResultType.Success,
                tenant,
                user
            );
        }

        #endregion

    }
}
