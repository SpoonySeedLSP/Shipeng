using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.Mvc.Results;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Events.Bus;
using Abp.Events.Bus.Exceptions;
using Abp.Logging;
using Abp.Runtime.Validation;
using Abp.Web.Models;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PearAdmin.AbpTemplate.Common.Reflection;
using System.Net;

namespace PearAdmin.AbpTemplate.Admin.Filter
{
    /// <summary>
    /// 异常拦截
    /// Abp 本身针对异常信息的核心处理就在于它的 AbpExceptionFilter 过滤器，过滤器实现很简单
    /// 它首先继承了 IExceptionFilter 接口，实现了其 OnException() 方法，
    /// 只要用户请求接口的时候出现了任何异常都会调用 OnException() 方法
    /// 而在 OnException() 方法内部，Abp 根据不同的异常类型进行了不同的异常处理
    /// </summary>
    public class AbpExceptionFilter : IExceptionFilter, ITransientDependency
    {
        // 日志记录器
        public ILogger Logger { get; set; }

        // 事件总线
        public IEventBus EventBus { get; set; }

        // 错误信息构建器
        private readonly IErrorInfoBuilder _errorInfoBuilder;
        // AspNetCore 相关的配置信息
        private readonly IAbpAspNetCoreConfiguration _configuration;

        // 注入并初始化内部成员对象
        public AbpExceptionFilter(IErrorInfoBuilder errorInfoBuilder, IAbpAspNetCoreConfiguration configuration)
        {
            _errorInfoBuilder = errorInfoBuilder;
            _configuration = configuration;

            Logger = NullLogger.Instance;
            EventBus = NullEventBus.Instance;
        }

        // 异常触发时会调用此方法
        public void OnException(ExceptionContext context)
        {
            // 判断是否由控制器触发，如果不是则不做任何处理
            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }

            // 获得方法的包装特性。决定后续操作,如果没有指定包装特性，则使用默认特性
            var wrapResultAttribute =
                ReflectionHelper.GetSingleAttributeOfMemberOrDeclaringTypeOrDefault(
                    context.ActionDescriptor.GetMethodInfo(),
                    _configuration.DefaultWrapResultAttribute
                );

            // 如果方法上面的包装特性要求记录日志，则记录日志
            if (wrapResultAttribute.LogError)
            {
                LogHelper.LogException(Logger, context.Exception);
            }

            // 如果被调用的方法上的包装特性要求重新包装错误信息，则调用 HandleAndWrapException() 方法进行包装
            if (wrapResultAttribute.WrapOnError)
            {
                HandleAndWrapException(context);
            }
        }

        // 处理并包装异常
        private void HandleAndWrapException(ExceptionContext context)
        {
            // 判断被调用接口的返回值是否符合标准，不符合则直接返回
            if (!ActionResultHelper.IsObjectResult(context.ActionDescriptor.GetMethodInfo().ReturnType))
            {
                return;
            }

            // 设置 HTTP 上下文响应所返回的错误代码，由具体异常决定。
            context.HttpContext.Response.StatusCode = GetStatusCode(context);

            // 重新封装响应返回的具体内容。采用 AjaxResponse 进行封装
            context.Result = new ObjectResult(
                new AjaxResponse(
                    _errorInfoBuilder.BuildForException(context.Exception),
                    context.Exception is AbpAuthorizationException
                )
            );

            // 触发异常处理事件
            EventBus.Trigger(this, new AbpHandledExceptionData(context.Exception));

            // 处理完成，将异常上下文的内容置为空
            context.Exception = null; //Handled!
        }

        // 根据不同的异常类型返回不同的 HTTP 错误码
        protected virtual int GetStatusCode(ExceptionContext context)
        {
            if (context.Exception is AbpAuthorizationException)
            {
                return context.HttpContext.User.Identity.IsAuthenticated
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;
            }

            if (context.Exception is AbpValidationException)
            {
                return (int)HttpStatusCode.BadRequest;
            }

            if (context.Exception is EntityNotFoundException)
            {
                return (int)HttpStatusCode.NotFound;
            }

            return (int)HttpStatusCode.InternalServerError;
        }
    }
}
