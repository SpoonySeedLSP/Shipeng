using Microsoft.AspNetCore.Mvc;
using Shipeng.Util;
using Shipeng.HRMS.Domain.ContentManage;
using Shipeng.HRMS.Service.ContentManage;
using Microsoft.AspNetCore.Authorization;

namespace Shipeng.HRMS.Web.Areas.ContentManage.Controllers
{
    /// <summary>
    /// 创 建：李仕鹏
    /// 日 期：2022-08-27 23:36
    /// 描 述：新闻类别控制器类
    /// </summary>
    [Area("ContentManage")]
    [AllowAnonymous]
    public class ArticleCategoryController :  ControllerBase
    {

        //属性注入示例
        public ArticleCategoryService _service { get; set; }
        #region 获取数据
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(Pagination pagination, string keyword)
        {
            if (string.IsNullOrEmpty(pagination.field))
            {
                pagination.order = "desc";
                pagination.field = "F_Id";
            }
            var data = await _service.GetLookList(pagination,keyword);
            return Success(pagination.records, data);
        }
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetTreeGridJson(string keyword)
        {
            var data = await _service.GetLookList();
            if (!string.IsNullOrEmpty(keyword))
            {
                data = data.TreeWhere(t => t.F_FullName.Contains(keyword));
            }
            return Success(data.Count, data);
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
            var data = await _service.GetForm(keyValue);
            return Content(data.ToJson());
        }
        #endregion

        #region 提交数据
        [HttpPost]
        [HandlerAjaxOnly]
        public async Task<ActionResult> SubmitForm(ArticleCategoryEntity entity, string keyValue)
        {
            try
            {
                await _service.SubmitForm(entity, keyValue);
                return await Success("操作成功。","",keyValue);
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
