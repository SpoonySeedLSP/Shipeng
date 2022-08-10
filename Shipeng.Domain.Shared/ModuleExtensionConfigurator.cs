using Volo.Abp.Threading;

namespace Shipeng.Domain.Shared
{
    public static class ModuleExtensionConfigurator
    {
        private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

        public static void Configure()
        {
            OneTimeRunner.Run(() =>
            {
                ConfigureExistingProperties();
                ConfigureExtraProperties();
            });
        }

        private static void ConfigureExistingProperties()
        {
            /* 可以更改的属性的最大长度
             * 应用程序使用的模块中定义的实体
             *
             * 示例：更改用户和角色名称的最大长度

               IdentityUserConsts.MaxNameLength = 99;
               IdentityRoleConsts.MaxNameLength = 99;

             * 注意：不建议更改属性长度
             * 除非你真的需要它，尽可能使用标准值
             *
             * 如果您使用的是EF Core，则需要在更改后运行add-migration命令
             */
        }

        private static void ConfigureExtraProperties()
        {
            /* 你可以配置额外的属性
             * 应用程序使用的模块中定义的实体
             *
             * 这个类可以用来定义这些额外的属性
             * 具有高级、易用的API
             *
             * 示例:为标识模块的用户实体添加一个新属性

               ObjectExtensionManager.Instance.Modules()
                  .ConfigureIdentity(identity =>
                  {
                      identity.ConfigureUser(user =>
                      {
                          user.AddOrUpdateProperty<string>( //property type: string
                              "SocialSecurityNumber", //property name
                              property =>
                              {
                                  //validation rules
                                  property.Attributes.Add(new RequiredAttribute());
                                  property.Attributes.Add(new StringLengthAttribute(64) {MinimumLength = 4});

                                  //...other configurations for this property
                              }
                          );
                      });
                  });

             * 有关更多信息，请参阅文档:
             * https://docs.abp.io/en/abp/latest/Module-Entity-Extensions
             */
        }
    }
}
