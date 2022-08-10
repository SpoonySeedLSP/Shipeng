namespace Shipeng.Domain.Data
{
    public interface IDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}
