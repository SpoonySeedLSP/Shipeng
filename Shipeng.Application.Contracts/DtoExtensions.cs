using Volo.Abp.Threading;

namespace Shipeng.Application.Contracts
{
    public static class DtoExtensions
    {
        private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

        public static void Configure()
        {
            OneTimeRunner.Run(() =>
            {
                /* 可以向Dtos添加扩展属性
                 * 在依赖模块中定义
                 *
                 * 例如:
                 *
                 * ObjectExtensionManager.Instance
                 *   .AddOrUpdateProperty<IdentityRoleDto, string>("Title");
                 *
                 * 有关更多信息，请参阅文档:
                 * https://docs.abp.io/en/abp/latest/Object-Extensions
                 */
            });
        }
    }
}
