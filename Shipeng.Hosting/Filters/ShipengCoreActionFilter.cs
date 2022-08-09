using Shipeng.Domain.Shared.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
namespace Shipeng.Hosting.Filters
{
    public class ShipengCoreActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var options = context.HttpContext.RequestServices.GetService<IOptions<ShipengGatewayOption>>();
            if (options != null)
            {
                var accessToken = options.Value.AccessToken;
                if (!context.HttpContext.Request.Headers.Where(x => x.Key == "AccessToken").Any())
                {
                    context.Result = new ContentResult()
                    {
                        Content = "授权访问Token为空",
                        StatusCode = 401
                    };
                }
                if (context.HttpContext.Request.Headers["AccessToken"].ToString() != accessToken)
                {
                    context.Result = new ContentResult()
                    {
                        Content = "授权访问Token错误",
                        StatusCode = 401
                    };
                }
            }
        }
    }
}
