using Shipeng;
using Shipeng.Dependency;
using Shipeng.UnifyResult;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Ӧ�÷��񼯺���չ�ࣨ�ɿ���ڲ����ã�
    /// </summary>
    [SuppressSniffer]
    public static class AppServiceCollectionExtensions
    {
        /// <summary>
        /// Mvc ע��������ã���Swagger��
        /// </summary>
        /// <param name="mvcBuilder">Mvc������</param>
        /// <param name="configure"></param>
        /// <returns>IMvcBuilder</returns>
        public static IMvcBuilder AddInject(this IMvcBuilder mvcBuilder, Action<InjectServiceOptions> configure = null)
        {
            mvcBuilder.Services.AddInject(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// ����ע��������ã���Swagger��
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <returns>IMvcBuilder</returns>
        /// <param name="configure"></param>
        public static IServiceCollection AddInject(this IServiceCollection services, Action<InjectServiceOptions> configure = null)
        {
            // �����������ѡ��
            var configureOptions = new InjectServiceOptions();
            configure?.Invoke(configureOptions);

            services.AddSpecificationDocuments(configureOptions?.SpecificationDocumentConfigure)
                    .AddDynamicApiControllers()
                    .AddDataValidation(configureOptions?.DataValidationConfigure)
                    .AddFriendlyException(configureOptions?.FriendlyExceptionConfigure);

            return services;
        }

        /// <summary>
        /// Mvc ע���������
        /// </summary>
        /// <param name="mvcBuilder">Mvc������</param>
        /// <param name="configure"></param>
        /// <returns>IMvcBuilder</returns>
        public static IMvcBuilder AddInjectBase(this IMvcBuilder mvcBuilder, Action<InjectServiceOptions> configure = null)
        {
            mvcBuilder.Services.AddInjectBase(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// Mvc ע���������
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <param name="configure"></param>
        /// <returns>IMvcBuilder</returns>
        public static IServiceCollection AddInjectBase(this IServiceCollection services, Action<InjectServiceOptions> configure = null)
        {
            // �����������ѡ��
            var configureOptions = new InjectServiceOptions();
            configure?.Invoke(configureOptions);

            services.AddDataValidation(configureOptions?.DataValidationConfigure)
                    .AddFriendlyException(configureOptions?.FriendlyExceptionConfigure);

            return services;
        }

        /// <summary>
        /// Mvc ע��������ú͹淶�����
        /// </summary>
        /// <param name="mvcBuilder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IMvcBuilder AddInjectWithUnifyResult(this IMvcBuilder mvcBuilder, Action<InjectServiceOptions> configure = null)
        {
            mvcBuilder.Services.AddInjectWithUnifyResult(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// ע��������ú͹淶�����
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddInjectWithUnifyResult(this IServiceCollection services, Action<InjectServiceOptions> configure = null)
        {
            services.AddInject(configure)
                    .AddUnifyResult();

            return services;
        }

        /// <summary>
        /// Mvc ע��������ú͹淶�����
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="mvcBuilder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IMvcBuilder AddInjectWithUnifyResult<TUnifyResultProvider>(this IMvcBuilder mvcBuilder, Action<InjectServiceOptions> configure = null)
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            mvcBuilder.Services.AddInjectWithUnifyResult<TUnifyResultProvider>(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// Mvc ע��������ú͹淶�����
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="configure"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInjectWithUnifyResult<TUnifyResultProvider>(this IServiceCollection services, Action<InjectServiceOptions> configure = null)
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            services.AddInject(configure)
                    .AddUnifyResult<TUnifyResultProvider>();

            return services;
        }

        /// <summary>
        /// �Զ������������
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppHostedService(this IServiceCollection services)
        {
            // ��ȡ���� BackgroundService ����
            var backgroundServiceTypes = App.EffectiveTypes.Where(u => typeof(BackgroundService).IsAssignableFrom(u));
            var addHostServiceMethod = typeof(ServiceCollectionHostedServiceExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .Where(u => u.Name.Equals("AddHostedService") && u.IsGenericMethod && u.GetParameters().Length == 1)
                                .FirstOrDefault();

            foreach (var type in backgroundServiceTypes)
            {
                addHostServiceMethod.MakeGenericMethod(type).Invoke(null, new object[] { services });
            }

            return services;
        }

        /// <summary>
        /// ������̨����������
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static void Build(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider(false);
            // �洢������
            InternalApp.RootServices = serviceProvider;
        }

        /// <summary>
        /// ���Ӧ������
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <param name="configure">��������</param>
        /// <returns>���񼯺�</returns>
        internal static IServiceCollection AddApp(this IServiceCollection services, Action<IServiceCollection> configure = null)
        {
            // ע��ȫ������ѡ��
            services.AddConfigurableOptions<AppSettingsOptions>();

            // ע���ڴ�ͷֲ�ʽ�ڴ�
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            // ע��ȫ������ע��
            services.AddDependencyInjection();

            // ע��ȫ�� Startup ɨ��
            services.AddStartups();

            // ��Ӷ���ӳ��
            services.AddObjectMapper();

            // �Զ������
            configure?.Invoke(services);

            return services;
        }

        /// <summary>
        /// ��� Startup �Զ�ɨ��
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <returns>���񼯺�</returns>
        internal static IServiceCollection AddStartups(this IServiceCollection services)
        {
            // ɨ�����м̳� AppStartup ����
            var startups = App.EffectiveTypes
                .Where(u => typeof(AppStartup).IsAssignableFrom(u) && u.IsClass && !u.IsAbstract && !u.IsGenericType)
                .OrderByDescending(u => GetStartupOrder(u));

            // ע���Զ��� startup
            foreach (var type in startups)
            {
                var startup = Activator.CreateInstance(type) as AppStartup;
                App.AppStartups.Add(startup);

                // ��ȡ���з�������ע���ʽ�ķ������緵��ֵvoid���ҵ�һ�������� IServiceCollection ����
                var serviceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(u => u.ReturnType == typeof(void)
                        && u.GetParameters().Length > 0
                        && u.GetParameters().First().ParameterType == typeof(IServiceCollection));

                if (!serviceMethods.Any()) continue;

                // �Զ���װ���Ե���
                foreach (var method in serviceMethods)
                {
                    method.Invoke(startup, new[] { services });
                }
            }

            return services;
        }

        /// <summary>
        /// ��ȡ Startup ����
        /// </summary>
        /// <param name="type">��������</param>
        /// <returns>int</returns>
        private static int GetStartupOrder(Type type)
        {
            return !type.IsDefined(typeof(AppStartupAttribute), true) ? 0 : type.GetCustomAttribute<AppStartupAttribute>(true).Order;
        }
    }
}