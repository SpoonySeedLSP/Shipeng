using Shipeng;
using Shipeng.Dependency;
using Shipeng.SpecificationDocument;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// �淶���ӿڷ�����չ��
    /// </summary>
    [SuppressSniffer]
    public static class SpecificationDocumentServiceCollectionExtensions
    {
        /// <summary>
        /// ��ӹ淶���ĵ�����
        /// </summary>
        /// <param name="mvcBuilder">Mvc ������</param>
        /// <param name="configure">�Զ�������</param>
        /// <returns>���񼯺�</returns>
        public static IMvcBuilder AddSpecificationDocuments(this IMvcBuilder mvcBuilder, Action<SpecificationDocumentServiceOptions> configure = default)
        {
            mvcBuilder.Services.AddSpecificationDocuments(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// ��ӹ淶���ĵ�����
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <param name="configure">�Զ�������</param>
        /// <returns>���񼯺�</returns>
        public static IServiceCollection AddSpecificationDocuments(this IServiceCollection services, Action<SpecificationDocumentServiceOptions> configure = default)
        {
            // �ж��Ƿ����ù淶���ĵ�
            if (App.Settings.InjectSpecificationDocument != true) return services;

            // �������
            services.AddConfigurableOptions<SpecificationDocumentSettingsOptions>();

            // �����������ѡ��
            var configureOptions = new SpecificationDocumentServiceOptions();
            configure?.Invoke(configureOptions);

            // ���Swagger����������
            services.AddSwaggerGen(options => SpecificationDocumentBuilder.BuildGen(options, configureOptions?.SwaggerGenConfigure));

            // ��� MiniProfiler ����
            AddMiniProfiler(services);

            return services;
        }

        /// <summary>
        /// ��� MiniProfiler ����
        /// </summary>
        /// <param name="services"></param>
        private static void AddMiniProfiler(IServiceCollection services)
        {
            // ע��MiniProfiler ���
            if (App.Settings.InjectMiniProfiler != true) return;

            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/index-mini-profiler";
                options.EnableMvcFilterProfiling = false;
                options.EnableMvcViewProfiling = false;
            });
        }
    }
}