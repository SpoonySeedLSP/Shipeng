using Shipeng.Domain.Shared;
using Volo.Abp.Threading;

namespace Shipeng.EntityFrameworkCore
{
    public static class EfCoreEntityExtensionMappings
    {
        private static readonly OneTimeRunner OneTimeRunner = new();

        public static void Configure()
        {
            GlobalFeatureConfigurator.Configure();
            ModuleExtensionConfigurator.Configure();

            OneTimeRunner.Run(() =>
            {
                /* 你可以配置额外的属性
                 * 应用程序使用的模块中定义的实体
                 *
                 * 这个类可以用来将这些额外的属性映射到数据库中的表字段
                 *
                 * 只使用这个类来配置与ef核心相关的映射
                 * 使用ModuleExtensionConfigurator类（在Domain.Shared项目中）
                 * 对于高级API，为所用模块的实体定义额外属性
                 *
                 * 示例:将一个属性映射到表字段::

                     ObjectExtensionManager.Instance
                         .MapEfCoreProperty<IdentityUser, string>(
                             "MyProperty",
                             (entityBuilder, propertyBuilder) =>
                             {
                                 propertyBuilder.HasMaxLength(128);
                             }
                         );

                 * 有关更多信息，请参阅文档:
                 * https://docs.abp.io/en/abp/latest/Customizing-Application-Modules-Extending-Entities
                 */
            });
        }
    }
}
