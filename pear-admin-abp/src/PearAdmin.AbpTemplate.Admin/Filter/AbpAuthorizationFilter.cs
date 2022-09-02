using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.Mvc.Results;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Events.Bus;
using Abp.Events.Bus.Exceptions;
using Abp.Web.Models;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Admin.Filter
{
    /// <summary>
    /// 权限过滤器
    /// 在权限拦截器与权限过滤器的内部实现都使用了IAuthorizationHelper的AuthorizeAsync()方法来进行权限校验
    /// </summary>
    public class AbpAuthorizationFilter : IAsyncAuthorizationFilter, ITransientDependency
    {
        public ILogger Logger { get; set; }

        // 权限验证类，这个才是真正针对权限进行验证的对象
        private readonly IAuthorizationHelper _authorizationHelper;
        // 异常包装器，主要是用来封装没有授权时返回的错误信息
        private readonly IErrorInfoBuilder _errorInfoBuilder;
        // 事件总线处理器，在这里用于触发一个未授权请求引发的事件，用户可以监听此事件来进行自己的处理
        private readonly IEventBus _eventBus;

        // 构造注入
        public AbpAuthorizationFilter(
            IAuthorizationHelper authorizationHelper,
            IErrorInfoBuilder errorInfoBuilder,
            IEventBus eventBus)
        {
            _authorizationHelper = authorizationHelper;
            _errorInfoBuilder = errorInfoBuilder;
            _eventBus = eventBus;
            Logger = NullLogger.Instance;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 如果注入了 IAllowAnonymousFilter 过滤器则允许所有匿名请求
            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
            {
                return;
            }

            // 如果不是一个控制器方法则直接返回
            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }

            // 开始使用 IAuthorizationHelper 来进行权限校验
            try
            {
                await _authorizationHelper.AuthorizeAsync(
                    context.ActionDescriptor.GetMethodInfo(),
                    context.ActionDescriptor.GetMethodInfo().DeclaringType
                );
            }
            // 如果是未授权异常的处理逻辑
            catch (AbpAuthorizationException ex)
            {
                // 记录日志
                Logger.Warn(ex.ToString(), ex);

                // 触发异常事件
                _eventBus.Trigger(this, new AbpHandledExceptionData(ex));

                // 如果接口的返回类型为 ObjectResult，则采用 AjaxResponse 对象进行封装信息
                if (ActionResultHelper.IsObjectResult(context.ActionDescriptor.GetMethodInfo().ReturnType))
                {
                    context.Result = new ObjectResult(new AjaxResponse(_errorInfoBuilder.BuildForException(ex), true))
                    {
                        StatusCode = context.HttpContext.User.Identity.IsAuthenticated
                            ? (int)System.Net.HttpStatusCode.Forbidden
                            : (int)System.Net.HttpStatusCode.Unauthorized
                    };
                }
                else
                {
                    context.Result = new ChallengeResult();
                }
            }
            // 其他异常则显示为内部异常信息
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), ex);

                _eventBus.Trigger(this, new AbpHandledExceptionData(ex));

                if (ActionResultHelper.IsObjectResult(context.ActionDescriptor.GetMethodInfo().ReturnType))
                {
                    context.Result = new ObjectResult(new AjaxResponse(_errorInfoBuilder.BuildForException(ex)))
                    {
                        StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                    };
                }
                else
                {
                    //TODO: How to return Error page?
                    context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.InternalServerError);
                }
            }
        }
    }
}
