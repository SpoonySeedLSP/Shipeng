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
            builder.UseSqlServer(connectionString)
                .ReplaceService<MigrationsSqlGenerator, AbpSqlserverMigrationsSqlGenerator>();//自定义数据库备注

            //builder.UseMySql(connectionString, MySqlServerVersion.LatestSupportedServerVersion);//MySQL
            //builder.UseNpgsql(connectionString);//PostgreSQL
            //builder.UseOracle(connectionString);//Oracle
        }

        public static void Configure(DbContextOptionsBuilder<AbpTemplateDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection)
                .ReplaceService<MigrationsSqlGenerator, AbpSqlserverMigrationsSqlGenerator>();//自定义数据库备注

            //builder.UseMySql(connection, MySqlServerVersion.LatestSupportedServerVersion);//MySQL
            //builder.UseNpgsql(connection);//PostgreSQL
            //builder.UseOracle(connection);//Oracle
        }
    }
}
