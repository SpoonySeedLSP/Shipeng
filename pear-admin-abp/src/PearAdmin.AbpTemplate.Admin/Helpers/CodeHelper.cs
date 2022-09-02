using Abp;

namespace PearAdmin.AbpTemplate.Admin.Helpers
{
    public class CodeHelper
    {
        /// <summary>
        /// 生成6位手机验证码
        /// </summary>
        /// <returns></returns>
        public static string SetNewPhoneCode()
        {
            var c = RandomHelper.GetRandom(100000, 999999);
            return c + "";
        }
    }
}
