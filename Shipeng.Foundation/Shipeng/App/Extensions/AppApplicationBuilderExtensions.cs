using Shipeng;
using Shipeng.Dependency;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Ӧ���м����չ�ࣨ�ɿ���ڲ����ã�
    /// </summary>
    [SuppressSniffer]
    public static class AppApplicationBuilderExtensions
    {
        /// <summary>
        /// ע������м������Swagger��
        /// </summary>
        /// <param name="app"></param>
        /// <param name="routePrefix">���ַ�����Ϊ��ҳ</param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseInject(this IApplicationBuilder app, string routePrefix = default, Action<InjectConfigureOptions> configure = null)
        {
            // �����м������ѡ��
            var configureOptions = new InjectConfigureOptions();
            configure?.Invoke(configureOptions);

            app.UseSpecificationDocuments(routePrefix, configureOptions?.SpecificationDocumentConfigure);

            return app;
        }

        /// <summary>
        /// ע������м��
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseInjectBase(this IApplicationBuilder app)
        {
            return app;
        }

        /// <summary>
        /// ���Ӧ���м��
        /// </summary>
        /// <param name="app">Ӧ�ù�����</param>
        /// <param name="configure">Ӧ������</param>
        /// <returns>Ӧ�ù�����</returns>
        internal static IApplicationBuilder UseApp(this IApplicationBuilder app, Action<IApplicationBuilder> configure = null)
        {
            // �����Զ������
            configure?.Invoke(app);
            return app;
        }
    }
}