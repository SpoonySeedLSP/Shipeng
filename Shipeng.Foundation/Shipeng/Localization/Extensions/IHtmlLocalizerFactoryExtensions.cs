using Shipeng;
using Shipeng.Dependency;
using Shipeng.Localization;

namespace Microsoft.AspNetCore.Mvc.Localization
{
    /// <summary>
    /// IHtmlLocalizerFactory ��չ��
    /// </summary>
    [SuppressSniffer]
    public static class IHtmlLocalizerFactoryExtensions
    {
        /// <summary>
        /// ����Ĭ�϶����Թ���
        /// </summary>
        /// <param name="htmlLocalizerFactory"></param>
        /// <returns></returns>
        public static IHtmlLocalizer Create(this IHtmlLocalizerFactory htmlLocalizerFactory)
        {
            var localizationSettings = App.GetOptions<LocalizationSettingsOptions>();
            return htmlLocalizerFactory.Create(localizationSettings.LanguageFilePrefix, localizationSettings.AssemblyName);
        }
    }
}