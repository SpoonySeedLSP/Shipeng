using Microsoft.Extensions.DependencyInjection;
using Shipeng.Domain;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp;
using Microsoft.EntityFrameworkCore;

namespace Shipeng.EntityFrameworkCore
{
    [DependsOn(
         typeof(DomainModule),
         typeof(AbpEntityFrameworkCoreSqliteModule)
         )]
    public class EntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<AbpDbContextOptions>(options =>
            {
                options.UseSqlite();
            });
            context.Services.AddAbpDbContext<ShipengDbContext>(options =>
            {
                options.AddDefaultRepositories(includeAllEntities: true);
            });
        }
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var dbContext = context.ServiceProvider.GetService<ShipengDbContext>();
            if (dbContext != null && dbContext.Database.GetMigrations().Any())
            {
                dbContext.Database.Migrate();
            }
        }
    }
}
