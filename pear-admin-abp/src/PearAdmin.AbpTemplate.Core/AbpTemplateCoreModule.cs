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

            #region ABP中多租户配置
            /* 1.启用/禁用多租户
             * 启用这一行来创建多租户应用程序
             * 如果不需要多租户，比如说没有多租户情形，应用部署在企业私有服务器上，那么也可以不考虑多租户的使用，
             * 可以在ABP中关闭多租户(尽管关闭了，但默认还是会使用一个租户，默认租户Id为1，此时只有这个默认租户没有宿主)
             * 在AbpTemplateCoreModule中PreInitialize方法内可以添加如下代码启用或关闭多租户，默认是启用的
             * 
             * 2.侦测当前租户并在Session中获取租户
             * ABP中租户名称是唯一的，对于识别当前租户或是宿主身份，ABP没有使用Asp.Net提供的Session，
             * 声明了IAbpSession接口并提供了默认的实现(ClaimAbpSession)去测定当前租户信息，
             * 按照如下思路去确定租户：
             * ①如果当前用户登录了系统，那么可以从当前用户的声明信息中读取到当前租户信息，如果没有读取到租户信息，那么可以断定是宿主
             * ②如果当前用户没有登录系统，那么会有几种方式去获取,如果以下几种方式仍未获取到租户Id，则认为是使用宿主登录
             * ③从当前域名或是子域名去获取域名名称，然后通过租户仓储去查询是否存在相关的域名或子域名存在则可以确定租户Id
             * ④从Http请求头中获取在ABP中默认配置项Abp.TenantId(该配置项可更换名称)
             * ⑤从Http请求的cookie中获取Abp.TenantId 
             * IAbpSession声明了获取当前用户和租户信息的方法，该方法允许我们获取当前登录的用户及当前的租户信息
             * 并且获取到的信息按照不同的规则，有着不同的作用
             * ①如果获取到的用户和租户Id都是空的，那么意味着当前用户没有登录系统，因此也无法断定出当前是宿主还是租户
             * ②如果用户Id是空的，但是租户Id不是空的，那么可以知道是哪个租户，但是用户仍然是没有登录的，只是选择了租户
             * ③如果用户Id不是空的，但是租户Id是空的，那么可以知道是用户使用宿主登录了系统
             * ④如果用户Id且租户Id不是空的，那么就知道是选择了租户并且是租户中的某个用户登录了系统
             * 
             * 3.数据过滤
             * 如果使用了多租户，那么在读取数据时，会依据当前租户Id加上额外的过滤条件，这一点ABP已经处理好了，
             * 我们无需在linq中敲代码，但是有个前提条件是，读取数据的这个实体有设置多租户
             * ①如果实体使用的是IMustHaveTenant接口，那么读取时会依照当前租户Id进行条件过滤
             * ②如果实体使用的是IMayHaveTenant接口，那么读取到的数据会依照当前租户Id的有无值进行区分，
             * 如果当前租户Id为空，那么将读取到宿主的数据，如果租户Id不为空，则读取相应租户数据
             * ③如果实体没使用这两个接口，则读取到的数据不区分宿主和租户
             * 这两个接口使用场景：如果是宿主和租户都需要的，比如角色、用户、部门等，
             * 那么使用IMayHaveTenant接口，如果仅是租户所需要的那只需使用IMustHaveTenant接口
             * 
             * 4.宿主与租户间切换
             * 此处切换可以这么理解，给我一个其它租户Id，我可以在我的租户中获取到其它租户的数据，
             * 相应的，其它租户也可以获取到我租户的数据，或是宿主获取租户数据
             * 如果不给定租户Id,租户可以获取宿主数据
             */
            #endregion
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
            //使用IocManager注入IAbpTemplateSqlExecuter
            IocManager.Resolve<IAbpTemplateSqlExecuter>();
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}
