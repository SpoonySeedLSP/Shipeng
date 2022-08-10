using Shipeng.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Shipeng.EntityFrameworkCore
{
    //[ConnectionStringName("Default")]
    [ConnectionString]
    public class ShipengDbContext : AbpDbContext<ShipengDbContext>
    {
        /* 
        * 在此处为聚合根/实体添加DbSet属性。
        */

        public ShipengDbContext(DbContextOptions<ShipengDbContext> options)
           : base(options)
        {

        }

        #region DbSet
        public DbSet<ServiceGovernanceConfigure> ServiceGovernanceConfigures { get; set; }
        public DbSet<AuthenticationConfigure> AuthenticationConfigures { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<Middleware> Middlewares { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Cluster> Clusters { get; set; }
        public DbSet<ClusterHealthCheck> ClusterHealthChecks { get; set; }
        public DbSet<RouteTransform> RouteTransforms { get; set; }
        public DbSet<ClusterDestination> ClusterDestinations { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Whitelist> Whitelists { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /* 在迁移数据库上下文中包含模块 */
            //builder.ConfigureSettingManagement();
            //builder.ConfigureAuditLogging();
            //builder.ConfigureIdentity();
            //builder.ConfigureTenantManagement();

            /* 在此处配置共享表（包括模块）*/

            //builder.Entity<AppUser>(b =>
            //{
            //    b.ToTable(AbpIdentityDbProperties.DbTablePrefix + "Users"); //Sharing the same table "AbpUsers" with the IdentityUser

            //    b.ConfigureByConvention();
            //    b.ConfigureAbpUser();

            //    /* Configure mappings for your additional properties
            //     * Also see the CodeEfCoreEntityExtensionMappings class
            //     */
            //});

            /* 在这里配置您自己的表/实体*/

            //builder.Entity<Roles>(b =>
            //{
            //    b.ToTable(Consts.DbTablePrefix + "Roles", Consts.DbSchema);
            //    b.ConfigureByConvention(); //auto configure for the base class props 自动配置基类道具

            //    /* Configure more properties here 在此处配置更多属性*/
            //    b.Property(x => x.Id).HasComment("主键，Guid").IsRequired();
            //    b.Property(x => x.RoleName).HasComment("角色名");
            //});
        }
    }
}
