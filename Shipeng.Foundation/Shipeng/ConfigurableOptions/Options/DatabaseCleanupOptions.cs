using System.Collections.Generic;

namespace Shipeng.ConfigurableOptions
{
    /// <summary>
    /// 数据库清理配置
    /// 注意：EnableExecute 默认必须为 false，避免误删生产数据。
    /// </summary>
    public class DatabaseCleanupOptions
    {
        /// <summary>
        /// 是否允许真正执行删除。
        /// false 时只能预览，不能删除。
        /// </summary>
        public bool EnableExecute { get; set; } = true;

        /// <summary>
        /// 源码根目录。
        /// MongoDB 集合名如果只写在 Service.cs 注释里，必须通过源码扫描才能拿到。
        /// </summary>
        public string SourceRoot { get; set; } = "G:\\代码\\云南鹰问智慧科技有限公司\\code\\YWTraceServer\\src";

        /// <summary>
        /// SQLServer 永远保留的表。
        /// </summary>
        public List<string> SqlServerKeepTables { get; set; } = new();

        /// <summary>
        /// MongoDB 永远保留的集合。
        /// </summary>
        public List<string> MongoKeepCollections { get; set; } = new();

        /// <summary>
        /// 手工补充的 MongoDB 使用中集合。
        /// 生产环境没有源码时，就把集合名写这里。
        /// </summary>
        public List<string> MongoUsedCollections { get; set; } = new();

        /// <summary>
        /// SQLServer 忽略前缀。
        /// </summary>
        public List<string> IgnoreTablePrefixes { get; set; } = new();

        /// <summary>
        /// MongoDB 忽略前缀。
        /// </summary>
        public List<string> IgnoreCollectionPrefixes { get; set; } = new();
    }
}
