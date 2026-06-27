using Shipeng;
using Shipeng.Dependency;
using Shipeng.Extensions;
using Shipeng.InstantMessaging;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// �յ�·�ɹ�������չ
    /// </summary>
    [SuppressSniffer]
    public static class IEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// ɨ���������м�����
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapHubs(this IEndpointRouteBuilder endpoints)
        {
            // ɨ�����м��������Ͳ������� [MapHub] �����Ҽ̳� Hub �� Hub<>
            var hubs = App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
                && u.IsDefined(typeof(MapHubAttribute), true)
                && (typeof(Hub).IsAssignableFrom(u) || u.HasImplementedRawGeneric(typeof(Hub<>))));

            if (!hubs.Any()) return;

            // �����ȡ MapHub ��չ����
            var mapHubMethod = typeof(HubEndpointRouteBuilderExtensions).GetMethods().Where(u => u.Name == "MapHub" && u.IsGenericMethod && u.GetParameters().Length == 3).FirstOrDefault();
            if (mapHubMethod == null) return;

            // �������м�������ע��
            foreach (var hub in hubs)
            {
                // ��������������
                var mapHubAttribute = hub.GetCustomAttribute<MapHubAttribute>(true);

                // �������ӷַ���ί��
                Action<HttpConnectionDispatcherOptions> configureOptions = options =>
                {
                    // ִ�����ӷַ���ѡ������
                    hub.GetMethod("HttpConnectionDispatcherOptionsSettings", BindingFlags.Public | BindingFlags.Static)
                        ?.Invoke(null, new object[] { options });
                };

                // ע�Ἧ����
                var hubEndpointConventionBuilder = mapHubMethod.MakeGenericMethod(hub).Invoke(null, new object[] { endpoints, mapHubAttribute.Pattern, configureOptions }) as HubEndpointConventionBuilder;

                // ִ���յ�ת��������
                hub.GetMethod("HubEndpointConventionBuilderSettings", BindingFlags.Public | BindingFlags.Static)
                    ?.Invoke(null, new object[] { hubEndpointConventionBuilder });
            }
        }
    }
}