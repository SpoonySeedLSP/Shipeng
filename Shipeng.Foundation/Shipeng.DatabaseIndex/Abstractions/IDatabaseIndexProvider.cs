namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// 数据库索引注册 Provider。
    ///
    /// 一个业务模块如果需要声明自己的 SQL Server / MongoDB 索引，
    /// 就新增一个类实现该接口，例如：
    /// 1. 商户套餐模块：PackageIndexProvider
    /// 2. 商户模块：MerchantIndexProvider
    /// 3. 认养模块：AdoptIndexProvider
    ///
    /// 注意：
    /// 1. Provider 只负责“声明需要哪些索引”，不负责真正创建索引。
    /// 2. 真正创建索引由 DatabaseIndexEnsureService 统一执行。
    /// 3. Provider 不要写业务查询、业务保存、业务校验逻辑。
    /// 4. Provider 类所在程序集必须被主程序引用或加载，否则自动扫描不到。
    /// </summary>
    public interface IDatabaseIndexProvider
    {
        /// <summary>
        /// 模块名称。
        ///
        /// 用途：
        /// 1. 日志展示时可以知道当前正在处理哪个模块的索引。
        /// 2. 执行结果中可以定位哪个模块索引创建失败。
        ///
        /// 示例：
        /// 商户套餐、商户管理、认养主体、作物档案。
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// 注册当前模块需要的数据库索引。
        ///
        /// 说明：
        /// 1. 这里只把索引定义添加到 plan 中。
        /// 2. 不要在这里直接执行 SQL。
        /// 3. 不要在这里直接调用 MongoDB 创建索引。
        /// 4. 不要在这里写业务逻辑。
        ///
        /// </summary>
        /// <param name="plan">数据库索引计划对象，用于收集 SQL Server 和 MongoDB 索引定义。</param>
        void Register(DatabaseIndexPlan plan);
    }
}