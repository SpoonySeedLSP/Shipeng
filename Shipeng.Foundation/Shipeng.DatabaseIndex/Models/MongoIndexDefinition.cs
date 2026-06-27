using MongoDB.Bson;
using MongoDB.Driver;

namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// MongoDB 索引定义基类。
    ///
    /// 为什么需要非泛型基类：
    /// DatabaseIndexPlan 需要把不同实体类型的 Mongo 索引放在同一个集合中统一处理。
    /// 例如：
    /// 1. MerchantPackageEntity
    /// 2. MerchantEntity
    /// 3. AdopterEntity
    /// 它们的泛型类型不同，因此需要一个共同基类。
    /// </summary>
    public abstract class MongoIndexDefinition
    {
        /// <summary>
        /// MongoDB 集合名称。
        /// </summary>
        public abstract string CollectionName { get; }

        /// <summary>
        /// MongoDB 索引名称。
        /// </summary>
        public abstract string IndexName { get; }

        /// <summary>
        /// 检查并创建 MongoDB 索引。
        ///
        /// 返回值：
        /// true  = 本次新建了索引。
        /// false = 索引已经存在，跳过。
        /// </summary>
        public abstract Task<bool> EnsureAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// MongoDB 泛型索引定义。
    ///
    /// 设计原则：
    /// 1. 先按索引名检查是否存在。
    /// 2. 已存在则跳过，不重复创建。
    /// 3. 不自动删除旧索引。
    ///
    /// 为什么不自动删除旧索引：
    /// 索引删除属于高风险操作，可能影响线上查询性能。
    /// 如果要调整索引字段，建议：
    /// 1. 新增一个新索引名。
    /// 2. 观察稳定后手动删除旧索引。
    /// </summary>
    public class MongoIndexDefinition<TEntity> : MongoIndexDefinition
    {
        /// <summary>
        /// MongoDB 集合对象。
        /// </summary>
        public IMongoCollection<TEntity> Collection { get; set; }

        /// <summary>
        /// MongoDB 集合名称。
        /// </summary>
        public override string CollectionName => Collection.CollectionNamespace.CollectionName;

        /// <summary>
        /// MongoDB 索引名称。
        /// </summary>
        public override string IndexName { get; }

        /// <summary>
        /// MongoDB 索引字段定义。
        /// </summary>
        public IndexKeysDefinition<TEntity> Keys { get; }

        /// <summary>
        /// 是否唯一索引。
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        /// 是否稀疏索引。
        ///
        /// 稀疏索引只索引存在该字段的文档。
        /// 适合可选字段。
        /// </summary>
        public bool Sparse { get; set; }

        /// <summary>
        /// TTL 过期时间。
        ///
        /// 适合日志、临时数据、验证码等自动过期场景。
        /// 普通业务数据不要设置。
        /// </summary>
        public TimeSpan? ExpireAfter { get; set; }

        /// <summary>
        /// 部分索引过滤条件。
        ///
        /// 示例：
        /// Builders&lt;TEntity&gt;.Filter.Eq(x => x.DeleteMark, null)
        ///
        /// 作用：
        /// 只给满足条件的数据建索引，减少索引体积。
        /// </summary>
        public FilterDefinition<TEntity>? PartialFilter { get; set; }

        /// <summary>
        /// 创建 MongoDB 索引定义。
        /// </summary>
        public MongoIndexDefinition(
            IMongoCollection<TEntity> collection,
            string indexName,
            IndexKeysDefinition<TEntity> keys)
        {
            Collection = collection ?? throw new ArgumentNullException(nameof(collection), "MongoDB 集合对象不能为空。");
            IndexName = indexName;
            Keys = keys ?? throw new ArgumentNullException(nameof(keys), "MongoDB 索引字段不能为空。");
        }

        /// <summary>
        /// 检查并创建 MongoDB 索引。
        /// </summary>
        public override async Task<bool> EnsureAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Validate();

            if (await ExistsAsync(cancellationToken))
                return false;

            var options = new CreateIndexOptions<TEntity>
            {
                Name = IndexName,
                Unique = Unique,
                Sparse = Sparse,
                ExpireAfter = ExpireAfter,
                PartialFilterExpression = PartialFilter
            };

            var model = new CreateIndexModel<TEntity>(Keys, options);
            await Collection.Indexes.CreateOneAsync(model, cancellationToken: cancellationToken);

            return true;
        }

        /// <summary>
        /// 校验 MongoDB 索引定义。
        /// </summary>
        private void Validate()
        {
            if (Collection == null)
                throw new ArgumentNullException(nameof(Collection), "MongoDB 集合对象不能为空。");

            if (string.IsNullOrWhiteSpace(IndexName))
                throw new ArgumentException("MongoDB 索引名不能为空。");

            if (Keys == null)
                throw new ArgumentNullException(nameof(Keys), "MongoDB 索引字段不能为空。");
        }

        /// <summary>
        /// 判断 MongoDB 索引是否已经存在。
        /// </summary>
        private async Task<bool> ExistsAsync(CancellationToken cancellationToken)
        {
            using var cursor = await Collection.Indexes.ListAsync(cancellationToken);
            var indexes = await cursor.ToListAsync(cancellationToken);

            foreach (var index in indexes)
            {
                if (!index.TryGetValue("name", out BsonValue? nameValue))
                    continue;

                if (nameValue.IsString &&
                    string.Equals(nameValue.AsString, IndexName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}