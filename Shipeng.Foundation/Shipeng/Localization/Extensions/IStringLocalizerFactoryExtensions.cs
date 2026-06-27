using Shipeng;
using Shipeng.Dependency;
using Shipeng.Localization;

namespace Microsoft.Extensions.Localization
{
    /// <summary>
    /// IStringLocalizerFactory ��չ��
    /// </summary>
    [SuppressSniffer]
    public static class IStringLocalizerFactoryExtensions
    {
        /// <summary>
        /// ����Ĭ�϶����Թ���
        /// </summary>
        /// <param name="stringLocalizerFactory"></param>
        /// <returns></returns>
        public static IStringLocalizer Create(this IStringLocalizerFactory stringLocalizerFactory)
        {
            var localizationSettings = App.GetOptions<LocalizationSettingsOptions>();
            return stringLocalizerFactory.Create(localizationSettings.LanguageFilePrefix, localizationSettings.AssemblyName);
        }
    }
}
