using Shipeng;
using Shipeng.CorsAccessor;
using Shipeng.Dependency;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// ������ʷ�����չ��
    /// </summary>
    [SuppressSniffer]
    public static class CorsAccessorServiceCollectionExtensions
    {
        /// <summary>
        /// ���ÿ���
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <param name="corsOptionsHandler"></param>
        /// <param name="corsPolicyBuilderHandler"></param>
        /// <returns>���񼯺�</returns>
        public static IServiceCollection AddCorsAccessor(this IServiceCollection services, Action<CorsOptions> corsOptionsHandler = default, Action<CorsPolicyBuilder> corsPolicyBuilderHandler = default)
        {
            // ��ӿ�������ѡ��
            services.AddConfigurableOptions<CorsAccessorSettingsOptions>();

            // ��ȡѡ��
            var corsAccessorSettings = App.GetConfig<CorsAccessorSettingsOptions>("CorsAccessorSettings", true);

            // ��ӿ������
            services.AddCors(options =>
            {
                // ��Ӳ��Կ���
                options.AddPolicy(name: corsAccessorSettings.PolicyName, builder =>
                {
                    // �ж��Ƿ���������Դ����Ϊ AllowAnyOrigin ���ܺ� AllowCredentialsһ����
                    var isNotSetOrigins = corsAccessorSettings.WithOrigins == null || corsAccessorSettings.WithOrigins.Length == 0;

                    // ���û��������Դ��������������Դ
                    if (isNotSetOrigins) builder.AllowAnyOrigin();
                    else builder.WithOrigins(corsAccessorSettings.WithOrigins)
                                      .SetIsOriginAllowedToAllowWildcardSubdomains();

                    // ���û�����������ͷ�����������б�ͷ
                    if (corsAccessorSettings.WithHeaders == null || corsAccessorSettings.WithHeaders.Length == 0) builder.AllowAnyHeader();
                    else builder.WithHeaders(corsAccessorSettings.WithHeaders);

                    // ���û�������κ�����ν�ʣ���������������ν��
                    if (corsAccessorSettings.WithMethods == null || corsAccessorSettings.WithMethods.Length == 0) builder.AllowAnyMethod();
                    else builder.WithMethods(corsAccessorSettings.WithMethods);

                    // ���ÿ���ƾ��
                    if (corsAccessorSettings.AllowCredentials == true && !isNotSetOrigins) builder.AllowCredentials();

                    // ������Ӧͷ
                    if (corsAccessorSettings.WithExposedHeaders != null && corsAccessorSettings.WithExposedHeaders.Length > 0) builder.WithExposedHeaders(corsAccessorSettings.WithExposedHeaders);

                    // ����Ԥ�����ʱ��
                    if (corsAccessorSettings.SetPreflightMaxAge.HasValue) builder.SetPreflightMaxAge(TimeSpan.FromSeconds(corsAccessorSettings.SetPreflightMaxAge.Value));

                    // ����Զ�������
                    corsPolicyBuilderHandler?.Invoke(builder);
                });

                // ����Զ�������
                corsOptionsHandler?.Invoke(options);
            });

            // �����Ӧѹ��
            services.AddResponseCaching();

            return services;
        }
    }
}