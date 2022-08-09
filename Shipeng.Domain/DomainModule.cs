using Shipeng.Domain.Shared;
using Volo.Abp.Modularity;

namespace Shipeng.Domain
{
    [DependsOn(
         typeof(DomainSharedModule)
     )]
    public class DomainModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
        }
    }
}
