namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// 数据库索引检查结果。
    ///
    /// 该对象用于：
    /// 1. 日志输出。
    /// 2. 后台接口返回执行结果。
    /// 3. 定位哪个索引创建失败。
    /// </summary>
    public class DatabaseIndexEnsureResult
    {
        /// <summary>
        /// 整体是否成功。
        ///
        /// true:
        /// 没有任何索引创建失败。
        ///
        /// false:
        /// 至少有一个 Provider 或索引创建失败。
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 扫描到的 Provider 数量。
        /// </summary>
        public int ProviderCount { get; set; }

        /// <summary>
        /// 成功处理的 Provider 数量。
        /// </summary>
        public int ProviderSuccessCount { get; set; }

        /// <summary>
        /// 处理失败的 Provider 数量。
        /// </summary>
        public int ProviderFailedCount { get; set; }

        /// <summary>
        /// SQL Server 索引定义总数。
        /// </summary>
        public int SqlServerIndexCount { get; set; }

        /// <summary>
        /// SQL Server 本次新建索引数量。
        /// </summary>
        public int SqlServerCreatedCount { get; set; }

        /// <summary>
        /// SQL Server 已存在并跳过的索引数量。
        /// </summary>
        public int SqlServerExistsCount { get; set; }

        /// <summary>
        /// MongoDB 索引定义总数。
        /// </summary>
        public int MongoIndexCount { get; set; }

        /// <summary>
        /// MongoDB 本次新建索引数量。
        /// </summary>
        public int MongoCreatedCount { get; set; }

        /// <summary>
        /// MongoDB 已存在并跳过的索引数量。
        /// </summary>
        public int MongoExistsCount { get; set; }

        /// <summary>
        /// 普通执行消息。
        /// </summary>
        public List<string> Messages { get; set; } = new();

        /// <summary>
        /// 错误消息。
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 添加普通消息。
        /// </summary>
        public void AddMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Messages.Add(message);
        }

        /// <summary>
        /// 添加错误消息，并把整体状态标记为失败。
        /// </summary>
        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Errors.Add(message);

            Success = false;
        }
    }
}