using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// 数据库索引检查与创建服务。
    ///
    /// 设计重点：
    /// 1. Provider 自动扫描后统一注入。
    /// 2. 每个 Provider 只负责注册索引计划。
    /// 3. SQL Server 和 MongoDB 索引都先检查是否存在。
    /// 4. 已存在的索引直接跳过，不重复创建。
    /// 5. DDL 操作顺序执行，避免启动瞬间对数据库造成过大压力。
    ///
    /// 为什么不并发创建索引：
    /// 1. SQL Server CREATE INDEX 可能产生锁。
    /// 2. MongoDB 创建索引也会消耗资源。
    /// 3. 启动阶段稳定性比极限速度更重要。
    /// 4. 已存在索引只做轻量检查，实际速度很快。
    /// </summary>
    public class DatabaseIndexEnsureService : IDatabaseIndexEnsureService
    {
        private readonly ISqlSugarClient _db;
        private readonly IEnumerable<IDatabaseIndexProvider> _providers;
        private readonly DatabaseIndexOptions _options;
        private readonly ILogger<DatabaseIndexEnsureService>? _logger;

        /// <summary>
        /// 构造函数。
        /// </summary>
        public DatabaseIndexEnsureService(
            ISqlSugarClient db,
            IEnumerable<IDatabaseIndexProvider> providers,
            IOptions<DatabaseIndexOptions> options,
            ILogger<DatabaseIndexEnsureService>? logger = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db), "SqlSugar 客户端不能为空。");
            _providers = providers ?? Enumerable.Empty<IDatabaseIndexProvider>();
            _options = options?.Value ?? new DatabaseIndexOptions();
            _logger = logger;
        }

        /// <summary>
        /// 检查并创建所有数据库索引。
        /// </summary>
        public async Task<DatabaseIndexEnsureResult> EnsureAsync(CancellationToken cancellationToken = default)
        {
            var result = new DatabaseIndexEnsureResult();

            var providers = _providers
                .Where(x => x != null)
                .OrderBy(x => GetProviderOrder(x))
                .ThenBy(x => x.ModuleName)
                .ToList();

            result.ProviderCount = providers.Count;

            if (providers.Count == 0)
            {
                result.AddMessage("未发现任何数据库索引 Provider。");
                return result;
            }

            _logger?.LogInformation("开始检查数据库索引，共发现 {Count} 个 Provider。", providers.Count);

            foreach (var provider in providers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var plan = new DatabaseIndexPlan();

                try
                {
                    provider.Register(plan);

                    result.AddMessage($"开始处理模块索引：{provider.ModuleName}");

                    await EnsureSqlServerIndexesAsync(provider, plan, result, cancellationToken);
                    await EnsureMongoIndexesAsync(provider, plan, result, cancellationToken);

                    result.ProviderSuccessCount++;
                    result.AddMessage($"模块索引处理完成：{provider.ModuleName}");
                }
                catch (Exception ex)
                {
                    result.ProviderFailedCount++;
                    result.AddError($"模块索引处理失败：{provider.ModuleName}，错误：{ex.Message}");
                    _logger?.LogError(ex, "模块索引处理失败：{ModuleName}", provider.ModuleName);

                    if (_options.ThrowWhenFailed)
                        throw;
                }
            }

            _logger?.LogInformation(
                "数据库索引检查完成。SQL Server 新建 {SqlCreated} 个，MongoDB 新建 {MongoCreated} 个，失败 {ErrorCount} 个。",
                result.SqlServerCreatedCount,
                result.MongoCreatedCount,
                result.Errors.Count);

            return result;
        }

        /// <summary>
        /// 创建 SQL Server 索引。
        /// </summary>
        private async Task EnsureSqlServerIndexesAsync(
            IDatabaseIndexProvider provider,
            DatabaseIndexPlan plan,
            DatabaseIndexEnsureResult result,
            CancellationToken cancellationToken)
        {
            result.SqlServerIndexCount += plan.SqlServerIndexes.Count;

            foreach (var sqlIndex in plan.SqlServerIndexes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var created = await sqlIndex.EnsureAsync(_db, cancellationToken);

                    if (created)
                    {
                        result.SqlServerCreatedCount++;
                        result.AddMessage($"SQL Server 已创建索引：{sqlIndex.IndexName}");
                    }
                    else
                    {
                        result.SqlServerExistsCount++;
                        result.AddMessage($"SQL Server 索引已存在：{sqlIndex.IndexName}");
                    }
                }
                catch (Exception ex)
                {
                    result.AddError($"SQL Server 创建索引失败，模块：{provider.ModuleName}，索引：{sqlIndex.IndexName}，错误：{ex.Message}");
                    _logger?.LogError(ex, "SQL Server 创建索引失败，模块：{ModuleName}，索引：{IndexName}", provider.ModuleName, sqlIndex.IndexName);

                    if (_options.ThrowWhenFailed)
                        throw;
                }
            }
        }

        /// <summary>
        /// 创建 MongoDB 索引。
        /// </summary>
        private async Task EnsureMongoIndexesAsync(
            IDatabaseIndexProvider provider,
            DatabaseIndexPlan plan,
            DatabaseIndexEnsureResult result,
            CancellationToken cancellationToken)
        {
            result.MongoIndexCount += plan.MongoIndexes.Count;

            foreach (var mongoIndex in plan.MongoIndexes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var created = await mongoIndex.EnsureAsync(cancellationToken);

                    if (created)
                    {
                        result.MongoCreatedCount++;
                        result.AddMessage($"MongoDB 已创建索引：{mongoIndex.CollectionName}.{mongoIndex.IndexName}");
                    }
                    else
                    {
                        result.MongoExistsCount++;
                        result.AddMessage($"MongoDB 索引已存在：{mongoIndex.CollectionName}.{mongoIndex.IndexName}");
                    }
                }
                catch (Exception ex)
                {
                    result.AddError($"MongoDB 创建索引失败，模块：{provider.ModuleName}，集合：{mongoIndex.CollectionName}，索引：{mongoIndex.IndexName}，错误：{ex.Message}");
                    _logger?.LogError(ex, "MongoDB 创建索引失败，模块：{ModuleName}，集合：{CollectionName}，索引：{IndexName}", provider.ModuleName, mongoIndex.CollectionName, mongoIndex.IndexName);

                    if (_options.ThrowWhenFailed)
                        throw;
                }
            }
        }

        /// <summary>
        /// 获取 Provider 排序。
        /// </summary>
        private static int GetProviderOrder(IDatabaseIndexProvider provider)
        {
            var attr = provider.GetType()
                .GetCustomAttributes(typeof(DatabaseIndexProviderAttribute), false)
                .OfType<DatabaseIndexProviderAttribute>()
                .FirstOrDefault();

            return attr?.Order ?? 0;
        }
    }
}