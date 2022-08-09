using Microsoft.Extensions.Options;
using Shipeng.Domain.Shared.Options;
using Shipeng.Domain.Authorization;

namespace Shipeng.Hosting.Middlewares
{
    public class ShipengAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly List<WhitelistOption> _whitelistOptions;
        private readonly AuthenticationOption _authenticationOption;
        private readonly IJwtTokenManager _jwtTokenManager;
        public ShipengAuthorizationMiddleware(RequestDelegate next
            , IOptions<List<WhitelistOption>> whitelistOptions, IJwtTokenManager jwtTokenManager, IOptions<AuthenticationOption> authenticationOption)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _whitelistOptions = whitelistOptions.Value;
            _jwtTokenManager = jwtTokenManager;
            _authenticationOption = authenticationOption.Value;
        }
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (!_authenticationOption.UseState)
            {
                await _next(context);
                return;
            }
            if (CheckWhitelist(context))
            {
                await _next(context);
                return;
            }
            //token验证
            var tokenResult= await _jwtTokenManager.ValidationTokenAsync(context);
            if (!tokenResult.Successed)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(tokenResult?.Message??"");
                return;
            }
            var claimTypes = new[] { "iat", "nbf", "exp", "iss", "aud" };
            var claims = tokenResult?.Claims?.Where(x => !claimTypes.Contains(x.Name))?.ToList();
            if (claims!=null && claims.Count>0)
            {
                foreach (var claim in claims)
                {
                    if (!string.IsNullOrWhiteSpace(claim.Name) && !context.Request.Headers.ContainsKey(claim.Name))
                    {
                        context.Request.Headers.Add(claim.Name, System.Net.WebUtility.UrlEncode(claim.Value));
                        context.Items.Add(claim.Name, System.Net.WebUtility.UrlEncode(claim.Value));
                    }
                }
            }           
            await _next(context);
            return;
        }
        private bool CheckWhitelist(HttpContext context)
        {
            var proxyFeature = context.GetReverseProxyFeature();
            //白名单验证
            var requestPath = context.Request.Path.Value?.ToLower();
            var reqeustMethod = context.Request.Method.ToUpper();

            //白名单优先匹配局部作用域
            if (_whitelistOptions.Any(x => x.RouteId == proxyFeature.Route.Config.RouteId))
            {
                //如果局部作用域中有带*的项则直接匹配通过
                if (_whitelistOptions.Any(x => x.RouteId == proxyFeature.Route.Config.RouteId && x.FilterText == "*"))
                {
                    return true;
                }
                //如果没有带*匹配的则每项都需要匹配
                if (_whitelistOptions.Any(x => x.RouteId == proxyFeature.Route.Config.RouteId
                    && !string.IsNullOrWhiteSpace(requestPath) && requestPath.Contains(x.FilterText.ToLower())
                    && x.RequestMethod.Contains(reqeustMethod)))
                {
                    return true;
                }
            }
            else
            {
                //如果全局作用域中有带*的项则直接匹配通过
                if (_whitelistOptions.Any(x => x.RouteId == "00000000-0000-0000-0000-000000000000" && x.FilterText == "*"))
                {
                    return true;
                }
                //匹配全局
                if (_whitelistOptions.Any(x => x.RouteId == "00000000-0000-0000-0000-000000000000"
                    && !string.IsNullOrWhiteSpace(requestPath) && requestPath.Contains(x.FilterText.ToLower())
                    && x.RequestMethod.Contains(reqeustMethod)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
