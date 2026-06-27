using Shipeng.ConfigurableOptions;
using Shipeng.Reflection;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Linq;
using Microsoft.OpenApi;

namespace Shipeng.SpecificationDocument
{
    /// <summary>
    /// 规范化文档配置选项
    /// </summary>
    public sealed class SpecificationDocumentSettingsOptions : IConfigurableOptions<SpecificationDocumentSettingsOptions>
    {
        /// <summary>
        /// 文档标题
        /// </summary>
        public string DocumentTitle { get; set; }

        /// <summary>
        /// 默认分组�?
        /// </summary>
        public string DefaultGroupName { get; set; }

        /// <summary>
        /// 启用授权支持
        /// </summary>
        public bool? EnableAuthorized { get; set; }

        /// <summary>
        /// 格式化为V2版本
        /// </summary>
        public bool? FormatAsV2 { get; set; }

        /// <summary>
        /// 配置规范化文档地址
        /// </summary>
        public string RoutePrefix { get; set; }

        /// <summary>
        /// 文档展开设置
        /// </summary>
        public DocExpansion? DocExpansionState { get; set; }

        /// <summary>
        /// XML 描述文件
        /// </summary>
        public string[] XmlComments { get; set; }

        /// <summary>
        /// 分组信息
        /// </summary>
        public SpecificationOpenApiInfo[] GroupOpenApiInfos { get; set; }

        /// <summary>
        /// 安全定义
        /// </summary>
        public SpecificationOpenApiSecurityScheme[] SecurityDefinitions { get; set; }

        /// <summary>
        /// 配置 Servers
        /// </summary>
        public OpenApiServer[] Servers { get; set; }

        /// <summary>
        /// 隐藏 Servers
        /// </summary>
        public bool? HideServers { get; set; }

        /// <summary>
        /// 默认 swagger.json 路由模板
        /// </summary>
        public string RouteTemplate { get; set; }

        /// <summary>
        /// 配置安装第三方包的分组名
        /// </summary>
        public string[] PackagesGroups { get; set; }

        /// <summary>
        /// 启用枚举 Schema 筛选器
        /// </summary>
        public bool? EnableEnumSchemaFilter { get; set; }

        /// <summary>
        /// 启用标签排序筛选器
        /// </summary>
        public bool? EnableTagsOrderDocumentFilter { get; set; }

        /// <summary>
        /// 是否开启访问swagger文档保护
        /// </summary>
        public bool? IsOpenDocsProtect { get; set; }

        /// <summary>
        /// 页面访问用户�?
        /// </summary>
        public string VisitUserName { get; set; }

        /// <summary>
        /// 页面访问密码
        /// </summary>
        public string VisitPassWord { get; set; }

        /// <summary>
        /// 后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public void PostConfigure(SpecificationDocumentSettingsOptions options, IConfiguration configuration)
        {
            options.DocumentTitle ??= "Specification Api Document";
            options.DefaultGroupName ??= "Default";
            options.FormatAsV2 ??= false;
            //options.RoutePrefix ??= "api";    // 可以通过 UseInject() 配置，所以注�?
            options.DocExpansionState ??= DocExpansion.List;

            // 加载项目注册和模块化/插件注释
            var frameworkPackageName = Reflect.GetAssemblyName(GetType());
            var projectXmlComments = App.Assemblies.Where(u => u.GetName().Name != frameworkPackageName).Select(t => t.GetName().Name);
            var externalXmlComments = App.ExternalAssemblies.Any() ? App.Settings.ExternalAssemblies.Select(u => u.EndsWith(".dll") ? u[0..^4] : u) : Array.Empty<string>();
            XmlComments ??= projectXmlComments.Concat(externalXmlComments).ToArray();

            GroupOpenApiInfos ??= new SpecificationOpenApiInfo[]
            {
                new SpecificationOpenApiInfo()
                {
                    Group=options.DefaultGroupName
                }
            };

            EnableAuthorized ??= true;
            if (EnableAuthorized == true)
            {
                //Swagger V2(OpenApi2.0)文档配置
                SecurityDefinitions ??= new SpecificationOpenApiSecurityScheme[]
                {
                    new SpecificationOpenApiSecurityScheme
                    {
                        Id="Bearer",//安全方案唯一标识
                        //Type= SecuritySchemeType.Http,
                        Type= SecuritySchemeType.ApiKey,//V2规范不支持Http鉴权类型，Bearer只能用ApiKey实现请求头鉴�?
                        Name="Authorization",
                        Description="JWT Authorization header using the Bearer scheme.",//参Http请求头Key名称：Authorization
                        BearerFormat="JWT",
                        Scheme="bearer",
                        In= ParameterLocation.Header,//参数位置：放在请求Header头部
                        // 接口全局安全引用配置
                        //Requirement=new SpecificationOpenApiSecurityRequirementItem
                        //{
                        //    Scheme=new OpenApiSecurityScheme
                        //    {
                        //        //引用上方Id=Bearer的安全定�?
                        //        Reference=new OpenApiReference
                        //        {
                        //            Id="Bearer",
                        //            Type= ReferenceType.SecurityScheme
                        //        }
                        //    },
                        //    Accesses=Array.Empty<string>()
                        //}
                        // 接口安全依赖引用
                        Requirement = new SpecificationOpenApiSecurityRequirementItem
                        {
                            //V2/V3统一简写，取消显式Reference引用
                            Accesses = Array.Empty<string>()
                         }
                    }
                };

            }

            Servers ??= Array.Empty<OpenApiServer>();
            HideServers ??= true;
            RouteTemplate ??= "swagger/{documentName}/swagger.json";
            PackagesGroups ??= Array.Empty<string>();
            EnableEnumSchemaFilter ??= true;
            EnableTagsOrderDocumentFilter ??= true;
        }

    }
}