using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Runtime.Security;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using PearAdmin.AbpTemplate.Authorization;
using PearAdmin.AbpTemplate.Authorization.Roles;
using PearAdmin.AbpTemplate.Authorization.Users;
using PearAdmin.AbpTemplate.Features;
using PearAdmin.AbpTemplate.Localization;
using PearAdmin.AbpTemplate.MultiTenancy;
using PearAdmin.AbpTemplate.Notifications;
using PearAdmin.AbpTemplate.Settings;
using PearAdmin.AbpTemplate.Social.Chat;
using PearAdmin.AbpTemplate.Social.Friendships;
using PearAdmin.AbpTemplate.Timing;

namespace PearAdmin.AbpTemplate
{
    [DependsOn(
        typeof(AbpZeroCoreModule),
        typeof(AbpAutoMapperModule)
        )]
    public class AbpTemplateCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            // 声明实体类型
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

            AbpTemplateLocalizationConfigurer.Configure(Configuration.Localization);

            // 启用这一行来创建多租户应用程序
            Configuration.MultiTenancy.IsEnabled = AbpTemplateCoreConsts.MultiTenancyEnabled;

            // 配置角色
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            // 添加设置提供程序
            Configuration.Settings.Providers.Add<AppSettingProvider>();

            // 添加功能提供程序
            Configuration.Features.Providers.Add<AppFeatureProvider>();

            // 添加通知提供程序
            Configuration.Notifications.Providers.Add<AppNotificationProvider>();

            // 添加权限提供程序
            Configuration.Authorization.Providers.Add<AppPermissionProvider>();

            Configuration.Settings.SettingEncryptionConfiguration.DefaultPassPhrase = AbpTemplateCoreConsts.DefaultPassPhrase;
            SimpleStringCipher.DefaultPassPhrase = AbpTemplateCoreConsts.DefaultPassPhrase;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpTemplateCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.RegisterIfNot<IChatCommunicator, NullChatCommunicator>();
            IocManager.Resolve<ChatUserStateWatcher>().Initialize();
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}
