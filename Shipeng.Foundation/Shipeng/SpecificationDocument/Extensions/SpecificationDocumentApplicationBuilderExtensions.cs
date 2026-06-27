using Shipeng;
using Shipeng.Dependency;
using Shipeng.SpecificationDocument;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// �淶���ĵ��м����չ
    /// </summary>
    [SuppressSniffer]
    public static class SpecificationDocumentApplicationBuilderExtensions
    {
        /// <summary>
        /// ��ӹ淶���ĵ��м��
        /// </summary>
        /// <param name="app"></param>
        /// <param name="routePrefix"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSpecificationDocuments(this IApplicationBuilder app, string routePrefix = default, Action<SpecificationDocumentConfigureOptions> configure = default)
        {
            // �ж��Ƿ����ù淶���ĵ�
            if (App.Settings.InjectSpecificationDocument != true) return app;

            // �����������ѡ��
            var configureOptions = new SpecificationDocumentConfigureOptions();
            configure?.Invoke(configureOptions);

            // ���� Swagger ȫ�ֲ���
            app.UseSwagger(options => SpecificationDocumentBuilder.Build(options, configureOptions?.SwaggerConfigure));

            // ���� Swagger UI ����
            app.UseSwaggerUI(options => SpecificationDocumentBuilder.BuildUI(options, routePrefix, configureOptions?.SwaggerUIConfigure));

            // ���� MiniProfiler���
            if (App.Settings.InjectMiniProfiler == true) app.UseMiniProfiler();

            return app;
        }
    }
}