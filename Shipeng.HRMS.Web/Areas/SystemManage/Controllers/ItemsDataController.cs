using Microsoft.AspNetCore.Mvc;
using Shipeng.Util;
using Shipeng.HRMS.Domain.SystemManage;
using Shipeng.HRMS.Service.SystemManage;

namespace Shipeng.HRMS.Web.Areas.SystemManage.Controllers
{
    [Area("SystemManage")]
    public class ItemsDataController : ControllerBase
    {

        public ItemsDataService _service { get; set; }
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(string itemId, string keyword)
        {
            var data =await _service.GetLookList(itemId, keyword);
            return Success(data.Count, data);
        }
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetSelectJson(string enCode)
        {
            var data =await _service.GetItemList(enCode);
            List<object> list = new List<object>();
            foreach (ItemsDetailEntity item in data)
            {
                list.Add(new { id = item.F_ItemCode, text = item.F_ItemName });
            }
            return Content(list.ToJson());
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
        public async Task<ActionResult> SubmitForm(ItemsDetailEntity itemsDetailEntity, string keyValue)
        {
            try
            {
                await _service.SubmitForm(itemsDetailEntity, keyValue);
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
