using System.ComponentModel;
using Shipeng.Dependency;

namespace Shipeng.EntityFrameworkCore
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    [SuppressSniffer]
    public enum DatabaseType
    {
        /// <summary>
        /// SqlServer数据库类型
        /// </summary>
        [Description("SqlServer")] SqlServer = 0,

        /// <summary>
        /// MySql数据库类型
        /// </summary>
        [Description("MySql")] MySql = 1,

        /// <summary>
        /// Oracle数据库类型
        /// </summary>
        [Description("Oracle")] Oracle = 2,

        /// <summary>
        /// Sqlite数据库类型
        /// </summary>
        [Description("Sqlite")] Sqlite = 3,

        /// <summary>
        /// PostgreSql数据库类型
        /// </summary>
        [Description("PostgreSql")] PostgreSql = 4
    }
}
