using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Shipeng.Domain.Configurations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using static Shipeng.Domain.Shared.CloudConsts;

namespace Shipeng.Hosting.SwaggerExtensions
{
    public static class ShipengSwaggerExtensions
    {
        /// <summary>
        /// 当前API版本，从appsettings.json获取
        /// </summary>
        private static readonly string version = $"v{AppSettings.ApiVersion}";

        /// <summary>
        /// Swagger描述信息
        /// </summary>
        private static readonly string description = @"<code>技术支持 .NET6</code><br/><br/><code>develop：痴情只为无情苦@一条渴死的鱼</code>";

        /// <summary>
        /// Swagger分组信息，将进行遍历使用
        /// </summary>
        private static readonly List<SwaggerApiInfo> ApiInfos = new()
        {
            new SwaggerApiInfo
            {
                UrlPrefix = Grouping.GroupName_v1,
                Name = "前台接口",
                OpenApiInfo = new OpenApiInfo
                {
                    Version = version,
                    Title = "Shipeng - 前台接口",
                    Description = description
                }
            },
            new SwaggerApiInfo
            {
                UrlPrefix = Grouping.GroupName_v2,
                Name = "后台接口",
                OpenApiInfo = new OpenApiInfo
                {
                    Version = version,
                    Title = "Shipeng - 后台接口",
                    Description = description
                }
            },
            new SwaggerApiInfo
            {
                UrlPrefix = Grouping.GroupName_v3,
                Name = "通用公共接口",
                OpenApiInfo = new OpenApiInfo
                {
                    Version = version,
                    Title = "Shipeng - 通用公共接口",
                    Description = description
                }
            },
            new SwaggerApiInfo
            {
                UrlPrefix = Grouping.GroupName_v4,
                Name = "JWT授权接口",
                OpenApiInfo = new OpenApiInfo
                {
                    Version = version,
                    Title = "Shipeng - JWT授权接口",
                    Description = description
                }
            }
        };

        /// <summary>
        /// AddSwagger
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            return services.AddSwaggerGen(options =>
            {
                // 遍历并应用Swagger分组信息
                ApiInfos.ForEach(x =>
                {
                    options.SwaggerDoc(x.UrlPrefix, x.OpenApiInfo);
                });

                // 应用Controller的API文档描述信息
                options.DocInclusionPredicate((docName, description) =>
                {
                    if (!description.TryGetMethodInfo(out MethodInfo method))
                    {
                        return false;
                    }

                    //使用ApiExplorerSettingsAttribute里面的GroupName进行特性标识
                    //DeclaringType只能获取controller上的特性              
                    var version = method.DeclaringType.GetCustomAttributes(true).OfType<ApiExplorerSettingsAttribute>().Select(m => m.GroupName);
                    if (version.Any())
                        return version.Any(v => v == docName);

                    // 获取action的特性
                    var actionVersion = method.GetCustomAttributes(true).OfType<ApiExplorerSettingsAttribute>().Select(m => m.GroupName);
                    if (actionVersion.Any())
                        return actionVersion.Any(v => v == docName);

                    return false;
                });

                // 注释说明
                var xmlPath = Path.Combine("Resources\\Shipeng.Hosting.xml");
                // 第二个参数设置为 true 显示 controller上的注释内容
                options.IncludeXmlComments(xmlPath, true);
            });
        }

        /// <summary>
        /// UseSwaggerUI
        /// </summary>
        /// <param name="app"></param>
        public static void UseSwaggerUI(this IApplicationBuilder app)
        {
            app.UseSwaggerUI(options =>
            {
                // 遍历分组信息，生成Json
                ApiInfos.ForEach(x =>
                {
                    options.SwaggerEndpoint($"/swagger/{x.UrlPrefix}/swagger.json", x.Name);
                });

                // 模型的默认扩展深度，设置为 -1 完全隐藏模型
                options.DefaultModelsExpandDepth(1);
                // API文档仅展开标记
                options.DocExpansion(DocExpansion.List);
                // 配置swagger Url
                options.RoutePrefix = "swagger";
                // API页面Title
                options.DocumentTitle = "😍Shipeng接口文档";
            });
        }

        internal class SwaggerApiInfo
        {
            /// <summary>
            /// URL前缀
            /// </summary>
            public string UrlPrefix { get; set; }

            /// <summary>
            /// 名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// <see cref="Microsoft.OpenApi.Models.OpenApiInfo"/>
            /// </summary>
            public OpenApiInfo OpenApiInfo { get; set; }
        }
    }
}