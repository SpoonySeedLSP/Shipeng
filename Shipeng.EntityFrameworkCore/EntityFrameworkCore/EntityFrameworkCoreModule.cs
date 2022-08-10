using Microsoft.Extensions.DependencyInjection;
using Shipeng.Domain;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.MySQL;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.EntityFrameworkCore.Oracle;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Shipeng.Domain.Configurations;

namespace Shipeng.EntityFrameworkCore
{
    [DependsOn(
        typeof(DomainModule),
        typeof(AbpIdentityEntityFrameworkCoreModule),
        typeof(AbpSettingManagementEntityFrameworkCoreModule),
        typeof(AbpEntityFrameworkCoreMySQLModule),
        typeof(AbpEntityFrameworkCorePostgreSqlModule),
        typeof(AbpEntityFrameworkCoreOracleModule),
        typeof(AbpEntityFrameworkCoreSqlServerModule),
        typeof(AbpEntityFrameworkCoreSqliteModule),
        typeof(AbpAuditLoggingEntityFrameworkCoreModule),
        typeof(AbpTenantManagementEntityFrameworkCoreModule)
        )]
    public class EntityFrameworkCoreModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            EfCoreEntityExtensionMappings.Configure();
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<ShipengDbContext>(options =>
            {
                /* 删除“includealentities:true”以创建
                 * 仅适用于聚合根的默认存储库 */
                options.AddDefaultRepositories(includeAllEntities: true);
            });

            Configure<AbpDbContextOptions>(options =>
            {
                /* 更改数据库管理系统的要点
                 * 有关EF核心工具，请参见CodeMigrationSbContextFactory */
                switch (AppSettings.EnableDb)
                {
                    case "MySql":
                        options.UseMySQL();
                        break;
                    case "PostgreSql":
                        options.UseNpgsql();
                        break;
                    case "Oracle":
                        options.UseOracle();
                        break;
                    case "SqlServer":
                        options.UseSqlServer();
                        break;
                    case "Sqlite":
                        options.UseSqlite();
                        break;
                    default:
                        options.UseSqlServer();
                        break;
                }
            });
        }

        //public override void OnApplicationInitialization(ApplicationInitializationContext context)
        //{
        //    var dbContext = context.ServiceProvider.GetService<ShipengDbContext>();
        //    if (dbContext != null && dbContext.Database.GetMigrations().Any())
        //    {
        //        dbContext.Database.Migrate();
        //    }
        //}
    }
}
