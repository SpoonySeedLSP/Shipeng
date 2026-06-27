using Shipeng;
using Shipeng.ConfigurableOptions;
using Shipeng.Dependency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// �ɱ�ѡ�������չ��
    /// </summary>
    [SuppressSniffer]
    public static class ConfigurableOptionsServiceCollectionExtensions
    {
        /// <summary>
        /// ���ѡ������
        /// </summary>
        /// <typeparam name="TOptions">ѡ������</typeparam>
        /// <param name="services">���񼯺�</param>
        /// <returns>���񼯺�</returns>
        public static IServiceCollection AddConfigurableOptions<TOptions>(this IServiceCollection services)
            where TOptions : class, IConfigurableOptions
        {
            var optionsType = typeof(TOptions);
            var optionsSettings = optionsType.GetCustomAttribute<OptionsSettingsAttribute>(false);

            // ��ȡ����·��
            var path = GetConfigurationPath(optionsSettings, optionsType);

            // ����ѡ�����֤��Ϣ��
            var configurationRoot = App.Configuration;
            var optionsConfiguration = configurationRoot.GetSection(path);

            // ����ѡ�����
            if (typeof(IConfigurableOptionsListener<TOptions>).IsAssignableFrom(optionsType))
            {
                var onListenerMethod = optionsType.GetMethod(nameof(IConfigurableOptionsListener<TOptions>.OnListener));
                if (onListenerMethod != null)
                {
                    // �����������ȫ�����ã��ܸо�����ͷ��
                    ChangeToken.OnChange(() => configurationRoot.GetReloadToken(), () =>
                    {
                        var options = optionsConfiguration.Get<TOptions>();
                        onListenerMethod.Invoke(options, new object[] { options, optionsConfiguration });
                    });
                }
            }

            var optionsConfigure = services.AddOptions<TOptions>()
                  .Bind(optionsConfiguration, options =>
                  {
                      options.BindNonPublicProperties = true; // ��˽�б���
                  })
                  .ValidateDataAnnotations();

            // ���ø�����֤���������
            var validateInterface = optionsType.GetInterfaces()
                .FirstOrDefault(u => u.IsGenericType && typeof(IConfigurableOptions).IsAssignableFrom(u.GetGenericTypeDefinition()));
            if (validateInterface != null)
            {
                var genericArguments = validateInterface.GenericTypeArguments;

                // ���ø�����֤
                if (genericArguments.Length > 1)
                {
                    services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IValidateOptions<TOptions>), genericArguments.Last()));
                }

                // ���ú�������
                var postConfigureMethod = optionsType.GetMethod(nameof(IConfigurableOptions<TOptions>.PostConfigure));
                if (postConfigureMethod != null)
                {
                    if (optionsSettings?.PostConfigureAll != true)
                        optionsConfigure.PostConfigure(options => postConfigureMethod.Invoke(options, new object[] { options, optionsConfiguration }));
                    else
                        services.PostConfigureAll<TOptions>(options => postConfigureMethod.Invoke(options, new object[] { options, optionsConfiguration }));
                }
            }

            return services;
        }

        /// <summary>
        /// ��ȡ����·��
        /// </summary>
        /// <param name="optionsSettings">ѡ����������</param>
        /// <param name="optionsType">ѡ������</param>
        /// <returns></returns>
        private static string GetConfigurationPath(OptionsSettingsAttribute optionsSettings, Type optionsType)
        {
            // Ĭ�Ϻ�׺
            var defaultStuffx = nameof(Options);

            return optionsSettings switch
            {
                // // û���� [OptionsSettings]�����ѡ������ `Options` ��β�����Ƴ������򷵻�������
                null => optionsType.Name.EndsWith(defaultStuffx) ? optionsType.Name[0..^defaultStuffx.Length] : optionsType.Name,
                // ������� [OptionsSettings] ���ԣ���δָ�� Path ��������ֱ�ӷ������������򷵻� Path
                _ => optionsSettings != null && string.IsNullOrWhiteSpace(optionsSettings.Path) ? optionsType.Name : optionsSettings.Path,
            };
        }
    }
}