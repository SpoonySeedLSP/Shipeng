using Shipeng.Application.Contracts;
using Shipeng.Domain;
using Volo.Abp.Modularity;

namespace Shipeng.Application
{
    [DependsOn(
        typeof(DomainModule),
        typeof(ApplicationContractsModule)
        )]
    public class ApplicationModule:AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            //在此处注入依赖项
        }
    }
}
