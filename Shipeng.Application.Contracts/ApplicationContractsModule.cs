using Shipeng.Domain.Shared;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace Shipeng.Application.Contracts
{
    [DependsOn(
       typeof(DomainSharedModule),
       typeof(AbpIdentityApplicationContractsModule),
       typeof(AbpPermissionManagementApplicationContractsModule),
       typeof(AbpSettingManagementApplicationContractsModule),
       typeof(AbpTenantManagementApplicationContractsModule),
       typeof(AbpObjectExtendingModule)
   )]
    public class ApplicationContractsModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            //在此处注入依赖项
            DtoExtensions.Configure();
        }
    }
}
