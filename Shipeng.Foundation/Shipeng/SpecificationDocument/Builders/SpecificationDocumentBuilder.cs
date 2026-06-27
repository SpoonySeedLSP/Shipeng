using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Shipeng.Dependency;
using Shipeng.DynamicApiController;
using Shipeng.Reflection;

namespace Shipeng.SpecificationDocument
{
    /// <summary>
    /// �淶���ĵ�������
    /// </summary>
    [SuppressSniffer]
    public static class SpecificationDocumentBuilder
    {
        /// <summary>
        /// �淶���ĵ�����
        /// </summary>
        private static readonly SpecificationDocumentSettingsOptions _specificationDocumentSettings;

        /// <summary>
        /// Ӧ��ȫ������
        /// </summary>
        private static readonly AppSettingsOptions _appSettings;

        /// <summary>
        /// ������Ϣ
        /// </summary>
        private static readonly IEnumerable<GroupExtraInfo> DocumentGroupExtras;

        /// <summary>
        /// ������ķ�����
        /// </summary>
        private static readonly Regex _groupOrderRegex;

        /// <summary>
        /// �ĵ������б�
        /// </summary>
        public static readonly IEnumerable<string> DocumentGroups;

        /// <summary>
        /// ���캯��
        /// </summary>
        static SpecificationDocumentBuilder()
        {
            // ��������
            _specificationDocumentSettings = App.GetOptions<SpecificationDocumentSettingsOptions>();
            _appSettings = App.Settings;

            // ��ʼ������
            _groupOrderRegex = new Regex(@"@(?<order>[0-9]+$)");
            GetActionGroupsCached = new ConcurrentDictionary<MethodInfo, IEnumerable<GroupExtraInfo>>();
            GetControllerGroupsCached = new ConcurrentDictionary<Type, IEnumerable<GroupExtraInfo>>();
            GetGroupOpenApiInfoCached = new ConcurrentDictionary<string, SpecificationOpenApiInfo>();
            GetControllerTagCached = new ConcurrentDictionary<ControllerActionDescriptor, string>();
            GetActionTagCached = new ConcurrentDictionary<ApiDescription, string>();

            // Ĭ�Ϸ��飬֧�ֶ�����ŷָ�
            DocumentGroupExtras = new List<GroupExtraInfo> { ResolveGroupExtraInfo(_specificationDocumentSettings.DefaultGroupName) };

            // �������з���
            DocumentGroups = ReadGroups();
        }

        /// <summary>
        /// ��鷽���Ƿ��ڷ�����
        /// </summary>
        /// <param name="currentGroup"></param>
        /// <param name="apiDescription"></param>
        /// <returns></returns>
        public static bool CheckApiDescriptionInCurrentGroup(string currentGroup, ApiDescription apiDescription)
        {
            if (!apiDescription.TryGetMethodInfo(out var method) || typeof(Controller).IsAssignableFrom(method.ReflectedType)) return false;

            return GetActionGroups(method).Any(u => u.Group == currentGroup);
        }

        /// <summary>
        /// ����Swaggerȫ������
        /// </summary>
        /// <param name="swaggerOptions">Swagger ȫ������</param>
        /// <param name="configure"></param>
        internal static void Build(SwaggerOptions swaggerOptions, Action<SwaggerOptions> configure = null)
        {
            // ����V2�汾
            //swaggerOptions.SerializeAsV2 = _specificationDocumentSettings.FormatAsV2 == true;

            // �������ö�̬�л�OpenApi�淶�汾
            swaggerOptions.OpenApiVersion = _specificationDocumentSettings.FormatAsV2 == true
                ? OpenApiSpecVersion.OpenApi2_0
                : OpenApiSpecVersion.OpenApi3_0;

            // �ж��Ƿ����� Server
            if (_specificationDocumentSettings.HideServers != true)
            {
                // ��������� Servers
                swaggerOptions.PreSerializeFilters.Add((swagger, request) =>
                {
                    // Ĭ�� Server
                    //var servers = new List<OpenApiServer> {
                    //    new OpenApiServer { Url = $"{request.Scheme}://{request.Host.Value}{_appSettings.VirtualPath}",Description="Default" }
                    //};
                    //servers.AddRange(_specificationDocumentSettings.Servers);

                    //V��PreSerializeFilters�������ĵ��汾��V2 ʹ��Host/BasePath��V3 ʹ��Servers
                    var servers = new List<OpenApiServer> {
                        new OpenApiServer {
                            Url = $"{request.Scheme}://{request.Host.Value}{_appSettings.VirtualPath}",
                            Description="Default"
                        }
                    };
                    //if (_specificationDocumentSettings.Servers is { Count: > 0 })
                    //    servers.AddRange(_specificationDocumentSettings.Servers);
                    if (_specificationDocumentSettings.Servers != null && _specificationDocumentSettings.Servers.Count() > 0) 
                        servers.AddRange(_specificationDocumentSettings.Servers);

                    swagger.Servers = servers;
                });
            }

            // ����·��ģ��
            swaggerOptions.RouteTemplate = _specificationDocumentSettings.RouteTemplate;

            // �Զ�������
            configure?.Invoke(swaggerOptions);
        }

        /// <summary>
        /// Swagger ����������
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger ����������</param>
        /// <param name="configure">�Զ�������</param>
        internal static void BuildGen(SwaggerGenOptions swaggerGenOptions, Action<SwaggerGenOptions> configure = null)
        {
            // ���������ĵ�
            CreateSwaggerDocs(swaggerGenOptions);

            // ���ط���������Ͷ��������б�
            LoadGroupControllerWithActions(swaggerGenOptions);

            // ���� Swagger SchemaId
            ConfigureSchemaId(swaggerGenOptions);

            // ���ñ�ǩ
            ConfigureTagsAction(swaggerGenOptions);

            // ���� Action ����
            ConfigureActionSequence(swaggerGenOptions);

            // ����ע�������ļ�
            LoadXmlComments(swaggerGenOptions);

            // ������Ȩ
            ConfigureSecurities(swaggerGenOptions);

            //ʹ�� Swagger �ܹ���ȷ����ʾ Enum �Ķ�Ӧ��ϵ
            if (_specificationDocumentSettings.EnableEnumSchemaFilter == true) swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();

            // ֧�ֿ������������
            if (_specificationDocumentSettings.EnableTagsOrderDocumentFilter == true) swaggerGenOptions.DocumentFilter<TagsOrderDocumentFilter>();

            // �Զ�������
            configure?.Invoke(swaggerGenOptions);
        }

        /// <summary>
        /// Swagger UI ����
        /// </summary>
        /// <param name="swaggerUIOptions"></param>
        /// <param name="routePrefix"></param>
        /// <param name="configure"></param>
        internal static void BuildUI(SwaggerUIOptions swaggerUIOptions, string routePrefix = default, Action<SwaggerUIOptions> configure = null)
        {
            // ���÷����յ�·��
            CreateGroupEndpoint(swaggerUIOptions);

            // �����ĵ�����
            swaggerUIOptions.DocumentTitle = _specificationDocumentSettings.DocumentTitle;

            // ����UI��ַ�������������Ŀ¼��
            swaggerUIOptions.RoutePrefix = _specificationDocumentSettings.RoutePrefix ?? routePrefix ?? "api";

            // �ĵ�չ������
            swaggerUIOptions.DocExpansion(_specificationDocumentSettings.DocExpansionState.Value);

            // ע�� MiniProfiler ���
            InjectMiniProfilerPlugin(swaggerUIOptions);

            // ���ö����Ժ��Զ���¼token
            AddDefaultInterceptor(swaggerUIOptions);

            // �Զ�������
            configure?.Invoke(swaggerUIOptions);
        }

        /// <summary>
        /// ���������ĵ�
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger����������</param>
        private static void CreateSwaggerDocs(SwaggerGenOptions swaggerGenOptions)
        {
            foreach (var group in DocumentGroups)
            {
                // ��ȡ�Զ����������ʵ��
                var groupOpenApiInfo = GetGroupOpenApiInfo(group) as OpenApiInfo;
                // ע������ĵ�
                swaggerGenOptions.SwaggerDoc(group, groupOpenApiInfo);
            }
        }

        /// <summary>
        /// ���ط���������Ͷ��������б�
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger ����������</param>
        private static void LoadGroupControllerWithActions(SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.DocInclusionPredicate(CheckApiDescriptionInCurrentGroup);
        }

        /// <summary>
        ///  ���ñ�ǩ
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        private static void ConfigureTagsAction(SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.TagActionsBy(apiDescription =>
            {
                return new[] { GetActionTag(apiDescription) };
            });
        }

        /// <summary>
        ///  ���� Action ����
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        private static void ConfigureActionSequence(SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.OrderActionsBy(apiDesc =>
            {
                var apiDescriptionSettings = apiDesc.CustomAttributes()
                                       .FirstOrDefault(u => u.GetType() == typeof(ApiDescriptionSettingsAttribute))
                                       as ApiDescriptionSettingsAttribute ?? new ApiDescriptionSettingsAttribute();

                return (int.MaxValue - apiDescriptionSettings.Order).ToString()
                                .PadLeft(int.MaxValue.ToString().Length, '0');
            });
        }

        /// <summary>
        /// ���� Swagger SchemaId
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger ����������</param>
        private static void ConfigureSchemaId(SwaggerGenOptions swaggerGenOptions)
        {
            // ���غ���
            static string DefaultSchemaIdSelector(Type modelType)
            {
                if (!modelType.IsConstructedGenericType) return modelType.Name;

                var prefix = modelType.GetGenericArguments()
                    .Select(genericArg => DefaultSchemaIdSelector(genericArg))
                    .Aggregate((previous, current) => previous + current);

                // ͨ�� _ ƴ�Ӷ������
                return modelType.Name.Split('`').First() + "_" + prefix;
            }

            // ���ñ��غ���
            swaggerGenOptions.CustomSchemaIds(modelType => DefaultSchemaIdSelector(modelType));
        }

        /// <summary>
        /// ����ע�������ļ�
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger ����������</param>
        private static void LoadXmlComments(SwaggerGenOptions swaggerGenOptions)
        {
            var xmlComments = _specificationDocumentSettings.XmlComments;
            foreach (var xmlComment in xmlComments)
            {
                var assemblyXmlName = xmlComment.EndsWith(".xml") ? xmlComment : $"{xmlComment}.xml";
                var assemblyXmlPath = Path.Combine(AppContext.BaseDirectory, assemblyXmlName);
                if (File.Exists(assemblyXmlPath))
                {
                    swaggerGenOptions.IncludeXmlComments(assemblyXmlPath, true);
                }
            }
        }

        /// <summary>
        /// ������Ȩ
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger ����������</param>
        private static void ConfigureSecurities(SwaggerGenOptions swaggerGenOptions)
        {
            // �ж��Ƿ���������Ȩ
            if (_specificationDocumentSettings.EnableAuthorized != true || _specificationDocumentSettings.SecurityDefinitions.Length == 0) return;

            var hasSecurityRequirement = false;

            // ���ɰ�ȫ����
            foreach (var securityDefinition in _specificationDocumentSettings.SecurityDefinitions)
            {
                // Id ���붨��
                if (string.IsNullOrWhiteSpace(securityDefinition.Id)) continue;

                // ��Ӱ�ȫ����
                var openApiSecurityScheme = securityDefinition as OpenApiSecurityScheme;
                swaggerGenOptions.AddSecurityDefinition(securityDefinition.Id, openApiSecurityScheme);

                //hasSecurityRequirement |= securityDefinition.Requirement?.Scheme != null;
                hasSecurityRequirement |= securityDefinition.Requirement != null;
            }

            // ��Ӱ�ȫ����
            if (hasSecurityRequirement)
            {
                swaggerGenOptions.AddSecurityRequirement(openApiDocument =>
                {
                    var openApiSecurityRequirement = new OpenApiSecurityRequirement();

                    foreach (var securityDefinition in _specificationDocumentSettings.SecurityDefinitions)
                    {
                        //if (string.IsNullOrWhiteSpace(securityDefinition.Id) || securityDefinition.Requirement?.Scheme == null) continue;
                        if (string.IsNullOrWhiteSpace(securityDefinition.Id) || securityDefinition.Requirement == null) continue;

                        var schemeReference = new OpenApiSecuritySchemeReference(securityDefinition.Id, openApiDocument, null);
                        openApiSecurityRequirement.Add(schemeReference, securityDefinition.Requirement.Accesses?.ToList() ?? new List<string>());
                    }

                    return openApiSecurityRequirement;
                });
            }
        }

        /// <summary>
        /// ���÷����յ�·��
        /// </summary>
        /// <param name="swaggerUIOptions"></param>
        private static void CreateGroupEndpoint(SwaggerUIOptions swaggerUIOptions)
        {
            foreach (var group in DocumentGroups)
            {
                var groupOpenApiInfo = GetGroupOpenApiInfo(group);

                // �滻·��ģ��
                var routeTemplate = _specificationDocumentSettings.RouteTemplate.Replace("{documentName}", Uri.EscapeDataString(group));
                swaggerUIOptions.SwaggerEndpoint($"{_appSettings.VirtualPath}/{routeTemplate}", groupOpenApiInfo?.Title ?? group);
            }
        }

        /// <summary>
        /// ע�� MiniProfiler ���
        /// </summary>
        /// <param name="swaggerUIOptions"></param>
        private static void InjectMiniProfilerPlugin(SwaggerUIOptions swaggerUIOptions)
        {
            // ���� MiniProfiler ���
            var thisType = typeof(SpecificationDocumentBuilder);
            var thisAssembly = thisType.Assembly;

            // �Զ��� Swagger ��ҳ
            var customIndex = $"{Reflect.GetAssemblyName(thisAssembly)}{thisType.Namespace.Replace("Shipeng", string.Empty)}.Assets.{(App.Settings.InjectMiniProfiler != true ? "index" : "index-mini-profiler")}.html";
            swaggerUIOptions.IndexStream = () => thisAssembly.GetManifestResourceStream(customIndex);
        }

        /// <summary>
        /// ���Ĭ������/��Ӧ������
        /// </summary>
        /// <param name="swaggerUIOptions"></param>
        private static void AddDefaultInterceptor(SwaggerUIOptions swaggerUIOptions)
        {
            // ���ö����Ժ��Զ���¼token
            swaggerUIOptions.UseRequestInterceptor("(request) => { return defaultRequestInterceptor(request); }");
            swaggerUIOptions.UseResponseInterceptor("(response) => { return defaultResponseInterceptor(response); }");
        }

        /// <summary>
        /// ��ȡ������Ϣ���漯��
        /// </summary>
        private static readonly ConcurrentDictionary<string, SpecificationOpenApiInfo> GetGroupOpenApiInfoCached;

        /// <summary>
        /// ��ȡ����������Ϣ
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private static SpecificationOpenApiInfo GetGroupOpenApiInfo(string group)
        {
            return GetGroupOpenApiInfoCached.GetOrAdd(group, Function);

            // ���غ���
            static SpecificationOpenApiInfo Function(string group)
            {
                return _specificationDocumentSettings.GroupOpenApiInfos.FirstOrDefault(u => u.Group == group) ?? new SpecificationOpenApiInfo { Group = group };
            }
        }

        /// <summary>
        /// ��ȡ���з�����Ϣ
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> ReadGroups()
        {
            // ��ȡ���еĿ������Ͷ�������
            var controllers = App.EffectiveTypes.Where(u => Penetrates.IsApiController(u));
            if (!controllers.Any()) return new[] { _specificationDocumentSettings.DefaultGroupName };

            var actions = controllers.SelectMany(c => c.GetMethods().Where(u => IsApiAction(u, c)));

            // �ϲ����з���
            var groupOrders = controllers.SelectMany(u => GetControllerGroups(u))
                .Union(
                    actions.SelectMany(u => GetActionGroups(u))
                )
                .Where(u => u != null && u.Visible)
                // �����ȡ�������
                .GroupBy(u => u.Group)
                .Select(u => new GroupExtraInfo
                {
                    Group = u.Key,
                    Order = u.Max(x => x.Order),
                    Visible = true
                });

            // ��������
            return groupOrders
                .OrderByDescending(u => u.Order)
                .ThenBy(u => u.Group)
                .Select(u => u.Group)
                .Union(_specificationDocumentSettings.PackagesGroups);
        }

        /// <summary>
        /// ��ȡ�������黺�漯��
        /// </summary>
        private static readonly ConcurrentDictionary<Type, IEnumerable<GroupExtraInfo>> GetControllerGroupsCached;

        /// <summary>
        /// ��ȡ�����������б�
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<GroupExtraInfo> GetControllerGroups(Type type)
        {
            return GetControllerGroupsCached.GetOrAdd(type, Function);

            // ���غ���
            static IEnumerable<GroupExtraInfo> Function(Type type)
            {
                // ���������û�ж��� [ApiDescriptionSettings] ���ԣ��򷵻�Ĭ�Ϸ���
                if (!type.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return DocumentGroupExtras;

                // ��ȡ����
                var apiDescriptionSettings = type.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
                if (apiDescriptionSettings.Groups == null || apiDescriptionSettings.Groups.Length == 0) return DocumentGroupExtras;

                // ������������Ϣ
                var groupExtras = new List<GroupExtraInfo>();
                foreach (var group in apiDescriptionSettings.Groups)
                {
                    groupExtras.Add(ResolveGroupExtraInfo(group));
                }

                return groupExtras;
            }
        }

        /// <summary>
        /// <see cref="GetActionGroups(MethodInfo)"/> ���漯��
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, IEnumerable<GroupExtraInfo>> GetActionGroupsCached;

        /// <summary>
        /// ��ȡ�������������б�
        /// </summary>
        /// <param name="method">����</param>
        /// <returns></returns>
        private static IEnumerable<GroupExtraInfo> GetActionGroups(MethodInfo method)
        {
            return GetActionGroupsCached.GetOrAdd(method, Function);

            // ���غ���
            static IEnumerable<GroupExtraInfo> Function(MethodInfo method)
            {
                // �����������û�ж��� [ApiDescriptionSettings] ���ԣ��򷵻����ڿ���������
                if (!method.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return GetControllerGroups(method.ReflectedType);

                // ��ȡ����
                var apiDescriptionSettings = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
                if (apiDescriptionSettings.Groups == null || apiDescriptionSettings.Groups.Length == 0) return GetControllerGroups(method.ReflectedType);

                // ��������
                var groupExtras = new List<GroupExtraInfo>();
                foreach (var group in apiDescriptionSettings.Groups)
                {
                    groupExtras.Add(ResolveGroupExtraInfo(group));
                }

                return groupExtras;
            }
        }

        /// <summary>
        /// <see cref="GetActionTag(ApiDescription)"/> ���漯��
        /// </summary>
        private static readonly ConcurrentDictionary<ControllerActionDescriptor, string> GetControllerTagCached;

        /// <summary>
        /// ��ȡ��������ǩ
        /// </summary>
        /// <param name="controllerActionDescriptor">�������ӿ�������</param>
        /// <returns></returns>
        private static string GetControllerTag(ControllerActionDescriptor controllerActionDescriptor)
        {
            return GetControllerTagCached.GetOrAdd(controllerActionDescriptor, Function);

            // ���غ���
            static string Function(ControllerActionDescriptor controllerActionDescriptor)
            {
                var type = controllerActionDescriptor.ControllerTypeInfo;
                // �����������û�ж��� [ApiDescriptionSettings] ���ԣ��򷵻����ڿ�������
                if (!type.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return controllerActionDescriptor.ControllerName;

                // ��ȡ��ǩ
                var apiDescriptionSettings = type.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
                return string.IsNullOrWhiteSpace(apiDescriptionSettings.Tag) ? controllerActionDescriptor.ControllerName : apiDescriptionSettings.Tag;
            }
        }

        /// <summary>
        /// <see cref="GetActionTag(ApiDescription)"/> ���漯��
        /// </summary>
        private static readonly ConcurrentDictionary<ApiDescription, string> GetActionTagCached;

        /// <summary>
        /// ��ȡ����������ǩ
        /// </summary>
        /// <param name="apiDescription">�ӿ�������</param>
        /// <returns></returns>
        private static string GetActionTag(ApiDescription apiDescription)
        {
            return GetActionTagCached.GetOrAdd(apiDescription, Function);

            // ���غ���
            static string Function(ApiDescription apiDescription)
            {
                if (!apiDescription.TryGetMethodInfo(out var method)) return "unknown";

                // ��ȡ������������
                var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;

                // �����������û�ж��� [ApiDescriptionSettings] ���ԣ��򷵻����ڿ�������
                if (!method.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return GetControllerTag(controllerActionDescriptor);

                // ��ȡ��ǩ
                var apiDescriptionSettings = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
                return string.IsNullOrWhiteSpace(apiDescriptionSettings.Tag) ? GetControllerTag(controllerActionDescriptor) : apiDescriptionSettings.Tag;
            }
        }

        /// <summary>
        /// �Ƿ��Ƕ�������
        /// </summary>
        /// <param name="method">����</param>
        /// <param name="ReflectedType">��������</param>
        /// <returns></returns>
        private static bool IsApiAction(MethodInfo method, Type ReflectedType)
        {
            // ���Ƿǹ��������󡢾�̬�����ͷ���
            if (!method.IsPublic || method.IsAbstract || method.IsStatic || method.IsGenericMethod) return false;

            // ����������Ͳ��ǿ������������ΪҲ������
            if (method.ReflectedType != ReflectedType || method.DeclaringType == typeof(object)) return false;

            // �����ܱ��������ԵĽӷ���
            if (method.IsDefined(typeof(ApiExplorerSettingsAttribute), true) && method.GetCustomAttribute<ApiExplorerSettingsAttribute>(true).IgnoreApi) return false;

            return true;
        }

        /// <summary>
        /// �������鸽����Ϣ
        /// </summary>
        /// <param name="group">������</param>
        /// <returns></returns>
        private static GroupExtraInfo ResolveGroupExtraInfo(string group)
        {
            string realGroup;
            var order = 0;

            if (!_groupOrderRegex.IsMatch(group)) realGroup = group;
            else
            {
                realGroup = _groupOrderRegex.Replace(group, "");
                order = int.Parse(_groupOrderRegex.Match(group).Groups["order"].Value);
            }

            var groupOpenApiInfo = GetGroupOpenApiInfo(realGroup);
            return new GroupExtraInfo
            {
                Group = realGroup,
                Order = groupOpenApiInfo.Order ?? order,
                Visible = groupOpenApiInfo.Visible ?? true
            };
        }
    }
}
