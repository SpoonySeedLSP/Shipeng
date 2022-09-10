using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using PearAdmin.AbpTemplate.Business.Enterprise;
using PearAdmin.AbpTemplate.Business.Enterprise.Dto;
using PearAdmin.AbpTemplate.MultiTenancy.TenantSetting;
using PearAdmin.AbpTemplate.MultiTenancy.TenantSetting.Dto;

namespace PearAdmin.AbpTemplate.Admin.Controllers
{
    /// <summary>
    /// 租户设置控制器
    /// </summary>
    [AbpMvcAuthorize]
    public class TenantSettingsController : AbpTemplateControllerBase
    {
        private readonly ITenantSettingsAppService _tenantSettingsAppService;
        private readonly IEnterpriseService _enterpriseService;//企业服务

        public TenantSettingsController(ITenantSettingsAppService tenantSettingsAppService,
            IEnterpriseService enterpriseService)
        {
            _tenantSettingsAppService = tenantSettingsAppService;
            _enterpriseService = enterpriseService;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            //获取当前用户id或null,如果没有用户登录，则可以为空
            if (!AbpSession.UserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            var settings = await _tenantSettingsAppService.GetAllSettings();
            //根据租户id和用户id获取企业信息
            if(!string.IsNullOrWhiteSpace(settings.CompanySettings?.CompanyName ?? ""))
            {
                var enterpriseInfo = await _enterpriseService.GetEnterpriseInfoAsync((int)AbpSession.TenantId, (long)AbpSession.UserId);
                settings.CompanySettings = ObjectMapper.Map<CompanySettingsEditDto>(enterpriseInfo);
            }            
            return View(settings);
        }

        /// <summary>
        /// 更新设置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> UpdateAllSettings([FromBody]TenantSettingsEditDto input)
        {
            await _tenantSettingsAppService.UpdateAllSettings(input);
            //保存企业信息
            var enterpriseInput= ObjectMapper.Map<EnterpriseInput>(input.CompanySettings);
            enterpriseInput.TenantId = AbpSession.TenantId;
            enterpriseInput.UserId = (long)AbpSession.UserId;
            await _enterpriseService.SaveEnterpriseInfoAsync(enterpriseInput);
            return Json(new { code = 200, msg = "更新设置成功" });
        }
    }
}