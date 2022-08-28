using Shipeng.HRMS.Service.SystemOrganize;
using Shipeng.Util;
using Shipeng.HRMS.Domain.SystemOrganize;
using Microsoft.AspNetCore.Mvc;

namespace Shipeng.HRMS.Web.Areas.SystemOrganize.Controllers
{
    [Area("SystemOrganize")]
    public class NoticeController : ControllerBase
    {

        public NoticeService _service { get; set; }

        [HandlerAjaxOnly]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> GetGridJson(SoulPage<NoticeEntity> pagination, string keyword)
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
            var data =await _service.GetList(keyword);
            return Content(data.ToJson());
        }
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
        {
            var data =await _service.GetLookForm(keyValue);
            return Content(data.ToJson());
        }
        [HttpPost]
        [HandlerAjaxOnly]
        public async Task<ActionResult> SubmitForm(NoticeEntity noticeEntity, string keyValue)
        {
            try
            {
                await _service.SubmitForm(noticeEntity, keyValue);
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
    }
}
