namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// 数据库索引检查与创建服务。
    ///
    /// 作用：
    /// 1. 收集所有 IDatabaseIndexProvider 注册的索引。
    /// 2. 检查 SQL Server 中索引是否已存在。
    /// 3. 检查 MongoDB 中索引是否已存在。
    /// 4. 不存在时自动创建。
    /// 5. 存在时跳过，避免重复创建导致异常。
    ///
    /// 典型调用场景：
    /// 1. 程序启动时由 DatabaseIndexHostedService 自动执行。
    /// 2. 后台管理接口手动执行索引初始化。
    /// 3. 新增业务模块后临时执行一次索引检查。
    /// </summary>
    public interface IDatabaseIndexEnsureService
    {
        /// <summary>
        /// 检查并创建所有已注册的数据库索引。
        /// </summary>
        /// <param name="cancellationToken">取消令牌，用于应用关闭时中断索引检查。</param>
        /// <returns>索引检查结果。</returns>
        Task<DatabaseIndexEnsureResult> EnsureAsync(CancellationToken cancellationToken = default);
    }
}