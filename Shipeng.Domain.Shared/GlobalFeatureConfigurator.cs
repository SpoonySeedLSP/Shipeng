using Volo.Abp.Threading;

namespace Shipeng.Domain.Shared
{
    public static class GlobalFeatureConfigurator
    {
        private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

        public static void Configure()
        {
            OneTimeRunner.Run(() =>
            {
                /* 您可以在此处配置（启用/禁用）所用模块的全局功能
                 *
                 * 如果不需要，您可以安全地删除此类并删除其用法
                 *
                 * 有关全局功能系统的更多信息，请参阅文档：
                 * https://docs.abp.io/en/abp/latest/Global-Features
                 */
            });
        }
    }
}
