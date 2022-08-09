using Volo.Abp.Modularity;

namespace Shipeng.Domain.Shared
{
    [DependsOn(

    )]
    public class DomainSharedModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            //在此处注入依赖项
        }
    }
}
