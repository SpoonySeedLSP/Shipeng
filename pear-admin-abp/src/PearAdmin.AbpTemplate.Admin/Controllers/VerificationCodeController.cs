using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Net.Sms;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PearAdmin.AbpTemplate.Admin.Helpers;
using PearAdmin.AbpTemplate.Authorization.Users;
using PearAdmin.AbpTemplate.MultiTenancy;
using PearAdmin.AbpTemplate.Validation;

namespace PearAdmin.AbpTemplate.Admin.Controllers
{
    public class VerificationCodeController : AbpTemplateControllerBase
    {
        private readonly UserManager _userManager;
        private readonly ISmsSender _smsSender;
        private readonly TenantManager _tenantManager;

        public VerificationCodeController(UserManager userManager,
            ISmsSender smsSender,
            TenantManager tenantManager)
        {
            _userManager = userManager;
            _smsSender = smsSender;
            _tenantManager = tenantManager;
        }
        #region 手机

        /// <summary>
        /// 注册时使用，发送手机验证码
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> SendMobileCode(string phoneNumber)
        {
            try
            {
                if (!ValidationHelper.IsPhoneNumber(phoneNumber))
                {
                    throw new UserFriendlyException("您输入了无效的手机号码！");
                }
                //检查手机是否已被注册
                var user = await _userManager.Users.Where(
                u => u.PhoneNumber == phoneNumber
                ).FirstOrDefaultAsync();

                if (user != null)
                {
                    throw new UserFriendlyException(L("OccupiedMobilePhone"));
                }
                //生成6位数字验证码
                var code = CodeHelper.SetNewPhoneCode();
                //发送验证码
                await _smsSender.SendAsync(phoneNumber, AbpTemplateCoreConsts.SmsTemplateCommonCode, "{\"code\":\"" + code + "\"}");
                //存储验证码、手机号、时间，以备验证使用
                //Session["PhoneCode"] = code;
                //Session["PhoneNumber"] = phoneNumber;
                //Session["PhoneCodeDateTime"] = DateTime.Now;
                return Json("OK");
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }



        /// <summary>
        ///验证手机时使用，发送手机验证码
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> SendMobileCodeByVerificationMobile(string phoneNumber)
        {
            try
            {
                if (!ValidationHelper.IsPhoneNumber(phoneNumber))
                {
                    throw new UserFriendlyException("您输入了无效的手机号码！");
                }
                //检查手机是否已被注册
                var user = await _userManager.Users.Where(
                u => u.PhoneNumber == phoneNumber && u.Id != AbpSession.UserId
                ).FirstOrDefaultAsync();

                if (user != null)
                {
                    throw new UserFriendlyException(L("OccupiedMobilePhone"));
                }
                //生成6位数字验证码
                var code = CodeHelper.SetNewPhoneCode();
                //发送验证码
                await _smsSender.SendAsync(phoneNumber, AbpTemplateCoreConsts.SmsTemplateCommonCode, "{\"code\":\"" + code + "\"}");
                //存储验证码、手机号、时间，以备验证使用
                //Session["PhoneCode"] = code;
                //Session["PhoneNumber"] = phoneNumber;
                //Session["PhoneCodeDateTime"] = DateTime.Now;
                return Json("OK");
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        #endregion

        #region private
        /// <summary>
        /// 检查手机号码是否已被注册，未注册抛出异常
        /// </summary>
        /// <param name="mobilePhone"></param>
        /// <returns></returns>
        private async Task<User> GetUserByCheckingMobile(string mobilePhone)
        {
            var user = await _userManager.Users.Where(
                u => u.PhoneNumber == mobilePhone
                ).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new UserFriendlyException(L("InvalidMobilePhone"));
            }

            return user;
        }

        #endregion
    }
}