using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shipeng.SpecificationDocument
{
    /// <summary>
    /// Swagger 文档 Basic 账号密码认证中间件。
    /// </summary>
    public sealed class SwaggerBasicAuthMiddleware
    {
        private const string SwaggerAuthCookieName = "Shipeng.SwaggerAuth";
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next"></param>
        public SwaggerBasicAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 中间件执行方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!IsSwaggerRequest(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var settings = App.GetOptions<SpecificationDocumentSettingsOptions>();
            //本地开发环境不启用 Swagger Basic 登录，避免调试时反复弹窗
            //if (App.WebHostEnvironment?.IsDevelopment() == true)
            //{
            //    await _next(context);
            //    return;
            //}

            // 这里必须判断开关。本地如果关闭保护，就不会一直弹窗。
            if (settings?.IsOpenDocsProtect != true)
            {
                await _next(context);
                return;
            }

            var userName = settings.VisitUserName?.Trim();
            var password = settings.VisitPassWord?.Trim();

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                await _next(context);
                return;
            }

            var expectedTicket = CreateTicket(userName, password);

            if (HasValidCookie(context, expectedTicket))
            {
                await _next(context);
                return;
            }

            if (IsAuthorized(context, userName, password))
            {
                context.Response.Cookies.Append(SwaggerAuthCookieName, expectedTicket, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });
                await _next(context);
                return;
            }

            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger\", charset=\"UTF-8\"";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Swagger document requires authentication.");
        }

        /// <summary>
        /// 判断是否为 Swagger 请求
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsSwaggerRequest(PathString path)
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
                    || value.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                    || value.EndsWith(".map", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 验证 Basic 认证信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static bool IsAuthorized(HttpContext context, string userName, string password)
        {
            var authorization = context.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authorization))
                return false;

            if (!authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                var encodedCredentials = authorization["Basic ".Length..].Trim();
                var credentialBytes = Convert.FromBase64String(encodedCredentials);
                var credentials = Encoding.UTF8.GetString(credentialBytes);

                var separatorIndex = credentials.IndexOf(':');
                if (separatorIndex <= 0)
                    return false;

                var inputUserName = credentials[..separatorIndex].Trim();
                var inputPassword = credentials[(separatorIndex + 1)..].Trim();

                return FixedEquals(inputUserName, userName)
                       && FixedEquals(inputPassword, password);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 验证是否存在有效的认证 Cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expectedTicket"></param>
        /// <returns></returns>
        private static bool HasValidCookie(HttpContext context, string expectedTicket)
        {
            if (!context.Request.Cookies.TryGetValue(SwaggerAuthCookieName, out var ticket))
                return false;

            return FixedEquals(ticket, expectedTicket);
        }

        /// <summary>
        /// 创建认证票据
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static string CreateTicket(string userName, string password)
        {
            var raw = $"{userName}:{password}:YWTraceSwagger";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 执行固定时间比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool FixedEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left ?? string.Empty);
            var rightBytes = Encoding.UTF8.GetBytes(right ?? string.Empty);

            return leftBytes.Length == rightBytes.Length
                   && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }

    }
}