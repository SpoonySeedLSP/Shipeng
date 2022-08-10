using Shipeng.Domain.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;

namespace Shipeng.EntityFrameworkCore
{
    public class EntityFrameworkCoreCodeDbSchemaMigrator : IDbSchemaMigrator, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityFrameworkCoreCodeDbSchemaMigrator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task MigrateAsync()
        {
            /* 我们有意解析MigrationsDbContext
                * 来自IServiceProvider(而不是直接注入)
                * 要正确获取中当前租户的连接字符串，请执行以下操作：
                * 当前范围
                */

            await _serviceProvider
                .GetRequiredService<ShipengDbContext>()
                .Database
                .MigrateAsync();
        }
    }
}