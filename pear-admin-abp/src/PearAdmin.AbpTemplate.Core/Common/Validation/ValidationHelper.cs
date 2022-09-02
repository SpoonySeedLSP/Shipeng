using System.Text.RegularExpressions;
using Abp.Extensions;

namespace PearAdmin.AbpTemplate.Validation
{
    public static class ValidationHelper
    {
        public const string EmailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string PhoneRegex = "^((13[0-9])|(14[5|7])|(15([0-3]|[5-9]))|(18[0,5-9]))\\d{8}$";

        /// <summary>
        /// 验证邮箱是否有效
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmail(string value)
        {
            if (value.IsNullOrEmpty())
            {
                return false;
            }
            var regex = new Regex(EmailRegex);
            return regex.IsMatch(value);
        }

        /// <summary>
        /// 验证手机号是否有效
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static bool IsPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }
            var regex = new Regex(PhoneRegex);
            return regex.IsMatch(phoneNumber);

        }

    }
}
