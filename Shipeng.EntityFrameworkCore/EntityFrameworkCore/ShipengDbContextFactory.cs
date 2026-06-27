using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shipeng.EntityFrameworkCore
{
    public class ShipengDbContextFactory : IDesignTimeDbContextFactory<ShipengDbContext>
    {
        public ShipengDbContext CreateDbContext(string[] args)
        {
            EfCoreEntityExtensionMappings.Configure();

            var configuration = BuildConfiguration();

            var EnableDb = configuration["ConnectionStrings:Enable"];

            var builder = new DbContextOptionsBuilder<ShipengDbContext>();

            switch (EnableDb)
            {
                case "MySql":
                    builder.UseMySQL(configuration.GetConnectionString(EnableDb));
                    break;
                case "PostgreSql":
                    builder.UseNpgsql(configuration.GetConnectionString(EnableDb));
                    break;
                case "Oracle":
                    builder.UseOracle(configuration.GetConnectionString(EnableDb));
                    break;
                case "SqlServer":
                    builder.UseSqlServer(configuration.GetConnectionString(EnableDb));
                    break;
                case "Sqlite":
                    builder.UseSqlite(configuration.GetConnectionString(EnableDb));
                    break;
            }
            return new ShipengDbContext(builder.Options);
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Shipeng.DbMigrator/"))
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}
