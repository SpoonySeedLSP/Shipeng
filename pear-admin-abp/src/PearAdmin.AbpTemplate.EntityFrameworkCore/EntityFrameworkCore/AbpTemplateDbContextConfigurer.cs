using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using PearAdmin.AbpTemplate.EntityFrameworkCore.EntityFrameworkCore;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore
{
    public static class AbpTemplateDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<AbpTemplateDbContext> builder, string connectionString)
        {
            //builder.UseMySql(connectionString, MySqlServerVersion.LatestSupportedServerVersion);
            builder.UseSqlServer(connectionString)
                .ReplaceService<MigrationsSqlGenerator, AbpSqlserverMigrationsSqlGenerator>();//自定义数据库备注
        }

        public static void Configure(DbContextOptionsBuilder<AbpTemplateDbContext> builder, DbConnection connection)
        {
            //builder.UseMySql(connection, MySqlServerVersion.LatestSupportedServerVersion);
            builder.UseSqlServer(connection)
                .ReplaceService<MigrationsSqlGenerator, AbpSqlserverMigrationsSqlGenerator>();//自定义数据库备注
        }
    }
}
