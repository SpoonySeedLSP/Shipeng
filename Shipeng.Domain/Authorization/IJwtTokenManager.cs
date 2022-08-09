using Microsoft.AspNetCore.Http;

namespace Shipeng.Domain.Authorization
{
    public interface IJwtTokenManager
    {
        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="httpContext">请求上下文</param>
        /// <returns></returns>
        Task<JwtTokenValidationResult> ValidationTokenAsync(HttpContext httpContext);
    }
}
