using Autofac;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Shipeng.HRMS.Service;
using Shipeng.Util;
using System.Reflection;

namespace Shipeng.HRMS.WebApi
{
    public class Startup : DefaultStartUp
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
        }
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddDefaultSwaggerGen(Assembly.GetExecutingAssembly().GetName().Name)
                    .AddSqlSugar()
                    .AddIf(GlobalContext.SystemConfig.RabbitMq.Enabled, x => x.AddWorkerService())
                    .AddDefaultAPI()
                    .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    });
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            AutofacConfigureContainer(builder, default, typeof(ControllerBase), typeof(IDenpendency), typeof(Program));
        }
        public override void Configure(IApplicationBuilder app)
        {
            base.Configure(app);
            //api全局异常
            app.UseMiddleware(typeof(GlobalExceptionMiddleware))
               .AddDefaultSwaggerGen()
               .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute("default", "api/{controller=ApiHome}/{action=Index}/{id?}");
                });
        }
    }
}
