using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Validation;
using Volo.Abp.AspNetCore.Mvc.ExceptionHandling;
using Shipeng.Hosting.Filters;
using Volo.Abp.Json;
using Yarp.ReverseProxy.Configuration;
using Shipeng.Domain.ReverseProxy;
using Shipeng.Domain.Shared.Options;
using Shipeng.Hosting.Middlewares;
using Microsoft.Extensions.Options;
using Shipeng.Application.Contracts.Dtos;
using Shipeng.Application.Contracts;
using Serilog;
using Microsoft.AspNetCore.HttpOverrides;
using Shipeng.Application;

namespace Shipeng.Hosting
{
    [DependsOn(
         typeof(AbpAutofacModule),
         typeof(ApplicationModule),
         typeof(AbpSwashbuckleModule)
     )]
    public class HostingModule:AbpModule
    {
        #region 中间件注入
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            context.Services.AddHttpClient();
            //注入会话
            context.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //
            ConfigureCore(context);
            ConfigureCors(context);
            ConfigureMvc(context);
            ConfigureReverseProxy(context);
        }
        /// <summary>
        /// 网关核心配置项
        /// </summary>
        /// <param name="context"></param>
        private void ConfigureCore(ServiceConfigurationContext context)
        {
            //注入网关基础配置
            context.Services.Configure<ShipengGatewayOption>(context.Services.GetConfiguration().GetSection("ShipengGateway"));
            //白名单配置
            context.Services.Configure<List<WhitelistOption>>(opt => { });
            //中间件配置
            context.Services.Configure<List<MiddlewareOption>>(opt => { });
            //Jwt身份认证配置
            context.Services.Configure<AuthenticationOption>(opt =>
            {
                opt.UseState = false;
            });
            context.Services.Configure<TokenValidationParameters>(opt => { });
            //Yarp反向代理配置
            context.Services.Configure<YarpOption>(opt => 
            {
                opt.Routes = new List<RouteOption>();
            });
        }
        /// <summary>
        /// MVC中间件注入配置
        /// </summary>
        /// <param name="context"></param>
        private void ConfigureMvc(ServiceConfigurationContext context)
        {
            context.Services.AddControllers(options =>
            {
                // 移除 AbpValidationActionFilter
                var filterMetadata = options.Filters.FirstOrDefault(x => x is ServiceFilterAttribute attribute && attribute.ServiceType.Equals(typeof(AbpValidationActionFilter)));
                if (filterMetadata != null)
                    options.Filters.Remove(filterMetadata);
                //移除全局异常过滤器
                var errIndex = options.Filters.ToList().FindIndex(filter => filter is ServiceFilterAttribute attr && attr.ServiceType.Equals(typeof(AbpExceptionFilter)));
                if (errIndex > -1)
                    options.Filters.RemoveAt(errIndex);
                //
                options.Filters.Add(typeof(AbpCoreExceptionFilter));
                options.Filters.Add<ShipengCoreActionFilter>();
            })
            .AddJsonOptions(opt => { });
            Configure<AbpJsonOptions>(options => options.DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>
        /// 配置反向代理
        /// </summary>
        /// <param name="context"></param>
        private void ConfigureReverseProxy(ServiceConfigurationContext context)
        {
            //注入反向代理
            context.Services.AddSingleton<IProxyConfigProvider, InDatabaseStoreConfigProvider>();
            context.Services.AddReverseProxy();
        }
        /// <summary>
        /// 跨域注入
        /// </summary>
        /// <param name="context"></param>
        private void ConfigureCors(ServiceConfigurationContext context)
        {
            //跨域配置
            context.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(o => true)
                    .AllowCredentials();
                });
            });
        }
        #endregion

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            
            LoadMainConfigureAsync(context);
            //
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseForwardedHeaders();
            app.UseCors();
            app.UseRouting();
           
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                endpoints.MapReverseProxy(proxyPipeline =>
                {
                    proxyPipeline.UseMiddleware<ShipengAuthorizationMiddleware>();
                    proxyPipeline.UseMiddleware<ShipengExternalMiddleware>();
                });
            });
        }

        /// <summary>
        /// 加载基础配置项
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private void LoadMainConfigureAsync(ApplicationInitializationContext context)
        {
            try
            {

                //初始化时刷新所有数据配置项
                var options = context.ServiceProvider.GetService<IOptions<ShipengGatewayOption>>();
                if (options != null)
                {
                    var httpClientFactory = context.ServiceProvider.GetService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient();
                    var configureResult =  httpClient.GetFromJsonAsync<ShipengResult<RefreshConfigureDto>>($"{options.Value.AdminServer}/api/Shipeng/refresh/configure").Result;
                    if (configureResult != null && configureResult.Code == 0)
                    {
                        var refreshAppService = context.ServiceProvider.GetService<IRefreshAppService>();
                        if (refreshAppService != null)
                        {
                           var res= configureResult.Data==null ? null : refreshAppService.RefreshConfigureAsync(configureResult.Data).Result;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }
    }
}
