using System;
using System.Collections.Generic;
using Abp.Authorization.Users;
using Abp.Extensions;

namespace PearAdmin.AbpTemplate.Authorization.Users
{
    public class User : AbpUser<User>
    {
        //默认密码
        public const string DefaultPassword = "abp@123456";
        public const int MaxMobilePhoneLength = 11;
        public const int MobileCodeLength = 6;

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,//用户名
                Surname = AdminUserName,//用户的姓
                EmailAddress = emailAddress,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            return user;
        }

        public static User CreateUser(int? tenantId)
        {
            return new User()
            {
                TenantId = tenantId,
            };
        }

        public Guid? ProfilePictureId { get; set; }

        /// <summary>
        /// 性别（0.男 1.女）
        /// </summary>
        public int Sex { get; set; } = 0;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
    }
}
