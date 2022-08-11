using Shipeng.Domain.Shared.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Shipeng.HttpApi.Controllers
{
    /* 从这个类继承控制器
    */
    public abstract class ShipengController : AbpController
    {
        protected ShipengController()
        {
            LocalizationResource = typeof(CodeResource);
        }
    }
}