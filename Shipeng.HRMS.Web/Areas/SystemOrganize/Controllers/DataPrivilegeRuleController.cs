using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shipeng.HRMS.Domain.SystemOrganize;
using Shipeng.HRMS.Service.SystemOrganize;
using Shipeng.Util;

namespace Shipeng.HRMS.Web.Areas.SystemOrganize.Controllers
{
    /// <summary>
    /// 创 建：李仕鹏
    /// 日 期：2022-08-27 23:36
    /// 描 述：数据权限控制器类
    /// </summary>
    [Area("SystemOrganize")]
    public class DataPrivilegeRuleController : ControllerBase
    {

        public DataPrivilegeRuleService _service { get; set; }

        #region 获取数据
        [HandlerAjaxOnly]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> GetGridJson(SoulPage<DataPrivilegeRuleEntity> pagination, string keyword)
        {
            if (string.IsNullOrEmpty(pagination.field))
            {
                pagination.field = "F_Id";
                pagination.order = "desc";
            }
            var data = await _service.GetLookList(pagination, keyword);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetListJson(string keyword)
        {
            var data = await _service.GetList(keyword);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
        {
            var data = await _service.GetLookForm(keyValue);
            return Content(data.ToJson());
        }
        [HttpGet]
        public virtual ActionResult RuleForm()
        {
            return View();
        }
        #endregion

        #region 提交数据
        [HttpPost]
        [HandlerAjaxOnly]
        public async Task<ActionResult> SubmitForm(DataPrivilegeRuleEntity entity, string listData, string keyValue)
        {
            var filterList = JsonConvert.DeserializeObject<List<FilterList>>(listData);
            foreach (var item in filterList)
            {
                if (!string.IsNullOrEmpty(item.Description))
                {
                    entity.F_Description += item.Description + ",";
                }
            }
            if (!string.IsNullOrEmpty(entity.F_Description))
            {
                entity.F_Description = entity.F_Description.Substring(0, entity.F_Description.Length - 1);
            }
            entity.F_PrivilegeRules = filterList.ToJson();
            try
            {
                await _service.SubmitForm(entity, keyValue);
                return await Success("操作成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        public async Task<ActionResult> DeleteForm(string keyValue)
        {
            try
            {
                await _service.DeleteForm(keyValue);
                return await Success("操作成功。", "", keyValue, DbLogType.Delete);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue, DbLogType.Delete);
            }
        }
        #endregion
    }
}
