using MongoDB.Driver;

namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// 数据库索引计划。
    ///
    /// 一个 Provider 在 Register 方法中把自己需要的索引添加到该对象中。
    /// DatabaseIndexEnsureService 再统一读取该对象并执行创建。
    ///
    /// 好处：
    /// 1. 业务模块只声明索引。
    /// 2. 索引执行逻辑统一维护。
    /// 3. SQL Server 和 MongoDB 可以成对注册，避免漏写。
    /// </summary>
    public class DatabaseIndexPlan
    {
        /// <summary>
        /// SQL Server 索引集合。
        /// </summary>
        public List<SqlServerIndexDefinition> SqlServerIndexes { get; } = new();

        /// <summary>
        /// MongoDB 索引集合。
        /// </summary>
        public List<MongoIndexDefinition> MongoIndexes { get; } = new();

        /// <summary>
        /// 注册 SQL Server 索引。
        /// </summary>
        public DatabaseIndexPlan SqlServerIndex(
            string tableName,
            string indexName,
            IEnumerable<SqlServerIndexColumn> columns,
            bool unique = false,
            string? filter = null,
            IEnumerable<string>? includeColumns = null)
        {
            SqlServerIndexes.Add(new SqlServerIndexDefinition
            {
                TableName = tableName,
                IndexName = indexName,
                Columns = columns?.ToList() ?? new List<SqlServerIndexColumn>(),
                Unique = unique,
                Filter = filter,
                IncludeColumns = includeColumns?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>()
            });

            return this;
        }

        /// <summary>
        /// 注册 MongoDB 索引。
        /// </summary>
        public DatabaseIndexPlan MongoIndex<TEntity>(
            IMongoCollection<TEntity> collection,
            string indexName,
            IndexKeysDefinition<TEntity> keys,
            bool unique = false,
            bool sparse = false,
            TimeSpan? expireAfter = null,
            FilterDefinition<TEntity>? partialFilter = null)
        {
            MongoIndexes.Add(new MongoIndexDefinition<TEntity>(collection, indexName, keys)
            {
                Unique = unique,
                Sparse = sparse,
                ExpireAfter = expireAfter,
                PartialFilter = partialFilter
            });

            return this;
        }

        /// <summary>
        /// 同时注册 SQL Server 和 MongoDB 索引。
        ///
        /// 适用场景：
        /// 同一个业务表同时有 SQL Server 表和 MongoDB 集合，
        /// 并且两边查询条件基本一致。
        ///
        /// 注意：
        /// 1. SQL Server 使用真实数据库字段名，例如 F_DeleteMark。
        /// 2. MongoDB 使用实体属性表达式，例如 x => x.DeleteMark。
        /// 3. 两边字段名不一定相同，所以这里仍然需要分别传。
        /// </summary>
        public DatabaseIndexPlan HybridIndex<TEntity>(
            string tableName,
            IMongoCollection<TEntity> collection,
            string indexName,
            IEnumerable<SqlServerIndexColumn> sqlColumns,
            IndexKeysDefinition<TEntity> mongoKeys,
            bool unique = false,
            string? sqlFilter = null,
            IEnumerable<string>? sqlIncludeColumns = null,
            bool mongoSparse = false,
            TimeSpan? mongoExpireAfter = null,
            FilterDefinition<TEntity>? mongoPartialFilter = null)
        {
            SqlServerIndex(
                tableName: tableName,
                indexName: indexName,
                columns: sqlColumns,
                unique: unique,
                filter: sqlFilter,
                includeColumns: sqlIncludeColumns);

            MongoIndex(
                collection: collection,
                indexName: indexName,
                keys: mongoKeys,
                unique: unique,
                sparse: mongoSparse,
                expireAfter: mongoExpireAfter,
                partialFilter: mongoPartialFilter);

            return this;
        }
    }
}