using Volo.Abp.DependencyInjection;

namespace Shipeng.Domain.Data
{
    /* 如果数据库提供程序未定义
     * IDbSchemaMigrator 实现
     */
    public class NullDbSchemaMigrator : IDbSchemaMigrator, ITransientDependency
    {
        public Task MigrateAsync()
        {
            return Task.CompletedTask;
        }
    }
}