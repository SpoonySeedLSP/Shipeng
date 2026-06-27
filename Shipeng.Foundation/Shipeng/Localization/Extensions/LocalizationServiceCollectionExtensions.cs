using Shipeng;
using Shipeng.Dependency;
using Shipeng.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// �����Է�����չ��
    /// </summary>
    [SuppressSniffer]
    public static class LocalizationServiceCollectionExtensions
    {
        /// <summary>
        /// ���ö����Է���
        /// </summary>
        /// <param name="mvcBuilde"></param>
        /// <returns></returns>
        public static IMvcBuilder AddAppLocalization(this IMvcBuilder mvcBuilde)
        {
            var services = mvcBuilde.Services;

            // ��Ӷ���������ѡ��
            services.AddConfigurableOptions<LocalizationSettingsOptions>();

            // ��ȡ����������ѡ��
            var localizationSettings = App.GetConfig<LocalizationSettingsOptions>("LocalizationSettings", true);

            // ���û�����ö�����ѡ���ע�����
            if (localizationSettings.SupportedCultures == null || localizationSettings.SupportedCultures.Length == 0) return mvcBuilde;

            // ע������Է���
            services.AddLocalization(options =>
            {
                if (!string.IsNullOrWhiteSpace(localizationSettings.ResourcesPath))
                    options.ResourcesPath = localizationSettings.ResourcesPath;
            });

            // ������ͼ�����Ժ���֤������
            mvcBuilde.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                     .AddDataAnnotationsLocalization(options =>
                     {
                         options.DataAnnotationLocalizerProvider = (type, factory) =>
                             factory.Create(localizationSettings.LanguageFilePrefix, localizationSettings.AssemblyName);
                     });

            // ע���������������ѡ��
            services.Configure((Action<RequestLocalizationOptions>)(options =>
            {
                Penetrates.SetRequestLocalization(options, localizationSettings);
            }));

            // ����������� Razor ��ͼ������������
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

            return mvcBuilde;
        }
    }
}