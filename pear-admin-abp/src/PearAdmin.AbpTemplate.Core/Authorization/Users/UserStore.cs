using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using PearAdmin.AbpTemplate.Authorization.Roles;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Authorization.Users
{
    public class UserStore : AbpUserStore<Role, User>
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public UserStore(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<User, long> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<UserLogin, long> userLoginRepository,
            IRepository<UserClaim, long> userClaimRepository,
            IRepository<UserPermissionSetting, long> userPermissionSettingRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository)
            : base(unitOfWorkManager,
                  userRepository,
                  roleRepository,
                  userRoleRepository,
                  userLoginRepository,
                  userClaimRepository,
                  userPermissionSettingRepository,
                  userOrganizationUnitRepository,
                  organizationUnitRoleRepository
                  )
        {
            _userRepository = userRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        /// <summary>
        /// 根据账号获取用户
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public virtual async Task<User> FindByAccountAsync(string account)
        {
            account = account.ToLower();
            return await _userRepository.FirstOrDefaultAsync(
                user => user.UserName.ToLower() == account
            );
        }

        #region 通过手机号查询用户方法
        /// <summary>
        /// 通过手机号查询用户
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns></returns>
        public virtual async Task<User> FindByPhoneNumberAsync(string phoneNumber)
        {
            return await _userRepository.FirstOrDefaultAsync(model => model.PhoneNumber == phoneNumber);
        }

        /// <summary>
        /// 通过手机号查询用户
        /// </summary>
        /// <param name="tenantId">租户id</param>
        /// <param name="phoneNumber">手机号</param>
        /// <returns></returns>
        [UnitOfWork]
        public virtual async Task<User> FindByPhoneNumberAsync(int? tenantId, string phoneNumber)
        {
            using (_unitOfWorkManager.Current.SetTenantId(tenantId))
            {
                return await FindByPhoneNumberAsync(phoneNumber);
            }
        }
        #endregion

    }
}
