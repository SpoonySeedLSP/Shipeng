using Shipeng;
using Shipeng.Dependency;
using Shipeng.DynamicApiController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///��̬�ӿڿ�������չ��
    /// </summary>
    [SuppressSniffer]
    public static class DynamicApiControllerServiceCollectionExtensions
    {
        /// <summary>
        /// ��Ӷ�̬�ӿڿ���������
        /// </summary>
        /// <param name="mvcBuilder">Mvc������</param>
        /// <returns>Mvc������</returns>
        public static IMvcBuilder AddDynamicApiControllers(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.Services.AddDynamicApiControllers();

            return mvcBuilder;
        }

        /// <summary>
        /// ��Ӷ�̬�ӿڿ���������
        /// </summary>
        /// <param name="services"></param>
        /// <returns>Mvc������</returns>
        public static IServiceCollection AddDynamicApiControllers(this IServiceCollection services)
        {
            var partManager = services.FirstOrDefault(s => s.ServiceType == typeof(ApplicationPartManager))?.ImplementationInstance as ApplicationPartManager
                ?? throw new InvalidOperationException($"`{nameof(AddDynamicApiControllers)}` must be invoked after `{nameof(MvcServiceCollectionExtensions.AddControllers)}`.");

            // ����ģ�黯/������򼯲���
            if (App.ExternalAssemblies.Any())
            {
                foreach (var assembly in App.ExternalAssemblies)
                {
                    partManager.ApplicationParts.Add(new AssemblyPart(assembly));
                }
            }

            // ��ӿ����������ṩ��
            partManager.FeatureProviders.Add(new DynamicApiControllerFeatureProvider());

            // �������
            services.AddConfigurableOptions<DynamicApiControllerSettingsOptions>();

            // ���� Mvc ѡ��
            services.Configure<MvcOptions>(options =>
            {
                // ���Ӧ��ģ��ת����
                options.Conventions.Add(new DynamicApiControllerApplicationModelConvention());
            });

            return services;
        }

        /// <summary>
        /// ����ⲿ���򼯲�������
        /// </summary>
        /// <param name="mvcBuilder">Mvc������</param>
        /// <param name="assemblies"></param>
        /// <returns>Mvc������</returns>
        public static IMvcBuilder AddExternalAssemblyParts(this IMvcBuilder mvcBuilder, IEnumerable<Assembly> assemblies)
        {
            // ������򼯲���
            if (assemblies != null && assemblies.Any())
            {
                foreach (var assembly in assemblies)
                {
                    mvcBuilder.AddApplicationPart(assembly);
                }
            }

            return mvcBuilder;
        }
    }
}