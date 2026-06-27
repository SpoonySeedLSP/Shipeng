using Shipeng.DataValidation;
using Shipeng.Dependency;
using Shipeng.UnifyResult.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Shipeng.UnifyResult
{
    /// <summary>
    /// RESTful 风格返回值
    /// </summary>
    [SuppressSniffer, UnifyModel(typeof(RESTfulResult<>))]
    public class RESTfulResultProvider : IUnifyResultProvider
    {
        /// <summary>
        /// 异常返回值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public IActionResult OnException(ExceptionContext context, ExceptionMetadata metadata)
        {
            return new JsonResult(RESTfulResult(metadata.StatusCode, errors: metadata.Errors));
        }

        /// <summary>
        /// 成功返回值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public IActionResult OnSucceeded(ActionExecutedContext context, object data)
        {
            return new JsonResult(RESTfulResult(StatusCodes.Status200OK, true, data, errors: "操作成功"));
        }

        /// <summary>
        /// 验证失败返回值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public IActionResult OnValidateFailed(ActionExecutingContext context, ValidationMetadata metadata)
        {
            return new JsonResult(RESTfulResult(StatusCodes.Status400BadRequest, errors: metadata.ValidationResult));
        }

        ///// <summary>
        ///// 特定状态码返回值
        ///// </summary>
        ///// <param name="context"></param>
        ///// <param name="statusCode"></param>
        ///// <param name="unifyResultSettings"></param>
        ///// <returns></returns>
        //public async Task OnResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings)
        //{
        //    // 设置响应状态码
        //    UnifyContext.SetResponseStatusCodes(context, statusCode, unifyResultSettings);

        //    switch (statusCode)
        //    {
        //        // 处理 401 状态码
        //        case StatusCodes.Status401Unauthorized:
        //            //await context.Response.WriteAsJsonAsync(RESTfulResult(600, errors: "登录过期,请重新登录")
        //            //    , App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
        //            await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "登录过期,请重新登录")
        //                , App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
        //            break;
        //        // 处理 403 状态码
        //        case StatusCodes.Status403Forbidden:
        //            await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "403 Forbidden")
        //                , App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
        //            break;
        //        case StatusCodes.Status405MethodNotAllowed:
        //            await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "405 Method Not Allowed")
        //              , App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
        //            break;
        //        case StatusCodes.Status500InternalServerError:
        //            await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "500 服务器的内部错误")
        //              , App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
        //            break;
        //        default: break;
        //    }
        //}

        /// <summary>
        /// 特定状态码返回值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="statusCode"></param>
        /// <param name="unifyResultSettings"></param>
        /// <returns></returns>
        public async Task OnResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings)
        {
            if (context == null)
                return;

            // Swagger / 静态文档资源不要走统一返回包装，否则 Basic 登录弹窗、index.html、swagger json 容易被改写
            if (IsSwaggerOrStaticDocumentRequest(context.Request.Path))
                return;

            // 响应已经开始后，不能再设置 StatusCode，也不能再 WriteAsJsonAsync，
            // 否则会报：Headers are read-only, response has already started.
            if (context.Response.HasStarted)
                return;

            // 设置响应状态码
            UnifyContext.SetResponseStatusCodes(context, statusCode, unifyResultSettings);

            if (context.Response.HasStarted)
                return;

            switch (statusCode)
            {
                // 处理 401 状态码
                case StatusCodes.Status401Unauthorized:
                    await context.Response.WriteAsJsonAsync(
                        RESTfulResult(statusCode, errors: "登录过期,请重新登录"),
                        App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                    break;

                // 处理 403 状态码
                case StatusCodes.Status403Forbidden:
                    await context.Response.WriteAsJsonAsync(
                        RESTfulResult(statusCode, errors: "403 Forbidden"),
                        App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                    break;

                // 处理 405 状态码
                case StatusCodes.Status405MethodNotAllowed:
                    await context.Response.WriteAsJsonAsync(
                        RESTfulResult(statusCode, errors: "405 Method Not Allowed"),
                        App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                    break;

                // 处理 500 状态码
                case StatusCodes.Status500InternalServerError:
                    await context.Response.WriteAsJsonAsync(
                        RESTfulResult(statusCode, errors: "500 服务器内部错误"),
                        App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 判断请求路径是否为 Swagger 文档相关的请求或静态资源请求。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsSwaggerOrStaticDocumentRequest(PathString path)
        {
            var value = path.Value;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.Equals("/", StringComparison.OrdinalIgnoreCase)
                   || value.Equals("/index.html", StringComparison.OrdinalIgnoreCase)
                   || value.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
                   || value.StartsWith("/swagger-ui", StringComparison.OrdinalIgnoreCase)
                   || value.StartsWith("/index-mini-profiler", StringComparison.OrdinalIgnoreCase)
                   || value.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                   || value.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                   || value.EndsWith(".ico", StringComparison.OrdinalIgnoreCase)
                   || value.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                   || value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                   || value.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                   || value.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                   || value.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                   || value.EndsWith(".map", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 返回 RESTful 风格结果集
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="succeeded"></param>
        /// <param name="data"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static RESTfulResult<object> RESTfulResult(int statusCode, bool succeeded = default, object data = default, object errors = default)
        {
            return new RESTfulResult<object>
            {
                code = statusCode,
                data = data,
                msg = errors,
                extras = UnifyContext.Take(),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }
    }
}