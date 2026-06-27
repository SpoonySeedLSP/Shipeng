using Shipeng;
using Shipeng.Dependency;
using Shipeng.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// ������������չ��
    /// </summary>
    [SuppressSniffer]
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Web ����ע��
        /// </summary>
        /// <param name="hostBuilder">Web����������</param>
        /// <param name="assemblyName">�ⲿ��������</param>
        /// <returns>IWebHostBuilder</returns>
        public static IWebHostBuilder Inject(this IWebHostBuilder hostBuilder, string assemblyName = default)
        {
            var frameworkPackageName = assemblyName ?? Reflect.GetAssemblyName(typeof(HostBuilderExtensions));
            hostBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, frameworkPackageName);

            return hostBuilder;
        }

        /// <summary>
        /// ��������ע��
        /// </summary>
        /// <param name="hostBuilder">��������ע�빹����</param>
        /// <param name="autoRegisterBackgroundService">�Ƿ��Զ�ע�� BackgroundService</param>
        /// <returns>IWebHostBuilder</returns>
        public static IHostBuilder Inject(this IHostBuilder hostBuilder, bool autoRegisterBackgroundService = true)
        {
            InternalApp.ConfigureApplication(hostBuilder, autoRegisterBackgroundService);

            return hostBuilder;
        }
    }
}