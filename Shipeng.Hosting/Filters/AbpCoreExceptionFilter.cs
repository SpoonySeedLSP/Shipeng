using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Shipeng.Application.Contracts;

namespace Shipeng.Hosting.Filters
{
    public class AbpCoreExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            Log.Error(context.Exception,context.Exception.Message);

            context.Result = new JsonResult(new ShipengResult() 
            {
                Code = 500,
                Message = context.Exception.Message
            });
            context.ExceptionHandled = true;
        }
    }
}
