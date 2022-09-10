using System;
using Abp.AspNetCore;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Dependency;
using Abp.Hangfire;
using Abp.Json;
using Hangfire;
using Hangfire.MemoryStorage;
using LogDashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using PearAdmin.AbpTemplate.Admin.Configuration;
using PearAdmin.AbpTemplate.Admin.Extensions;
using PearAdmin.AbpTemplate.Admin.Extensions.Filters;
using PearAdmin.AbpTemplate.Admin.Filter;
using PearAdmin.AbpTemplate.Admin.SignalR;
using PearAdmin.AbpTemplate.Authorization;
using PearAdmin.AbpTemplate.Debugging;
using PearAdmin.AbpTemplate.Identity;

namespace PearAdmin.AbpTemplate.Admin
{
    public class Startup
    {
        private readonly IConfigurationRoot Configuration;

        public Startup(IWebHostEnvironment env)
        {
            Configuration = env.GetAppConfiguration();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<FormOptions>(options =>
            {
                //限制为256MB
                options.MultipartBodyLengthLimit = 268435456;
            });

            #region MVC
            services.AddControllersWithViews(options =>//添加带视图的控制器
            {
                options.Filters.Add(typeof(AbpAuthorizationFilter));//权限过滤器
                options.Filters.Add(typeof(AbpExceptionFilter));//异常拦截过滤器
                //自动验证防伪令牌属性特性
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                //Abp自动验证防伪令牌特性
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            })
                .AddRazorRuntimeCompilation()//添加Razor运行时编译
                .AddNewtonsoftJson(options =>
                {
                    //忽略循环引用
                    //options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    //不使用驼峰样式的key  不更改元数据的key的大小写
                    //options.SerializerSettings.ContractResolver = new DefaultContractResolver(); //NullToEmptyStringResolver();
                    //定义时间格式
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    options.SerializerSettings.ContractResolver = new AbpMvcContractResolver(IocManager.Instance)
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };
                });
            services.Configure<MvcNewtonsoftJsonOptions>(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";//对类型为DateTime的生效
            });
            #endregion

            #region Identity
            services.Configure<IdentityOptions>(options =>
            {
                options.User.AllowedUserNameCharacters = null;//修改用户名验证规则
            });

            IdentityRegistrar.Register(services);
            #endregion

            #region SignalR
            services.AddSignalR();
            #endregion

            #region Hangfire
            services.AddHangfire(options =>
            {
                if (DebugHelper.IsDebug)
                {
                    options.UseMemoryStorage();
                }
                else
                {
                    var redisConnectionString = Configuration.GetConnectionString(AbpTemplateCoreConsts.RedisConnectionStringName);
                    options.UseRedisStorage(redisConnectionString);
                }
            });
            #endregion

            #region LogDashboard
            services.AddLogDashboard(options =>
            {
                options.AddAuthorizationFilter(new AbpLogDashboardAuthorizationFilter(AppPermissionNames.Pages_SystemManagement_HangfireDashboard));
            });
            #endregion

            return services.AddAbp<AbpTemplateAdminModule>(AbpBootstrapperOptionsExtension.GetOptions(Configuration));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAbp();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[]
                {
                    new AbpHangfireAuthorizationFilter(AppPermissionNames.Pages_SystemManagement_HangfireDashboard)
                }
            });

            app.UseLogDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AbpCommonHub>("/signalr");
                endpoints.MapHub<ChatHub>("/signalr-chat");
                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
