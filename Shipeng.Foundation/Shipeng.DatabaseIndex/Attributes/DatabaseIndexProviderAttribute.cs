namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// 数据库索引 Provider 标记特性。
    ///
    /// 说明：
    /// 1. 该特性不是必须的。
    /// 2. 只要类实现 IDatabaseIndexProvider，就会被自动扫描。
    /// 3. 加这个特性主要是为了让代码更清晰，也可以指定排序。
    ///
    /// 示例：
    /// [DatabaseIndexProvider("商户套餐", 100)]
    /// public class PackageIndexProvider : IDatabaseIndexProvider
    /// {
    /// }
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DatabaseIndexProviderAttribute : Attribute
    {
        /// <summary>
        /// 模块名称。
        /// 如果为空，则默认使用 Provider.ModuleName。
        /// </summary>
        public string? ModuleName { get; }

        /// <summary>
        /// 扫描排序。
        /// 数值越小越靠前。
        /// 一般不需要设置，只有索引创建顺序有特殊要求时才使用。
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// 创建数据库索引 Provider 标记。
        /// </summary>
        /// <param name="moduleName">模块名称。</param>
        /// <param name="order">排序号。</param>
        public DatabaseIndexProviderAttribute(string? moduleName = null, int order = 0)
        {
            ModuleName = moduleName;
            Order = order;
        }
    }
}