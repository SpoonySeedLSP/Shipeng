using SqlSugar;
using System.Data;
using System.Text;

namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// SQL Server 索引排序方向。
    /// </summary>
    public enum SqlServerIndexSortType
    {
        /// <summary>
        /// 升序。
        /// </summary>
        Asc = 0,

        /// <summary>
        /// 降序。
        /// </summary>
        Desc = 1
    }

    /// <summary>
    /// SQL Server 索引字段定义。
    /// </summary>
    public class SqlServerIndexColumn
    {
        /// <summary>
        /// 数据库真实字段名。
        ///
        /// 示例：
        /// F_DeleteMark、F_LastModifyTime、F_SortCode。
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// 排序方向。
        /// </summary>
        public SqlServerIndexSortType SortType { get; set; } = SqlServerIndexSortType.Asc;

        /// <summary>
        /// 创建升序索引字段。
        /// </summary>
        public static SqlServerIndexColumn Asc(string columnName)
        {
            return new SqlServerIndexColumn
            {
                ColumnName = columnName,
                SortType = SqlServerIndexSortType.Asc
            };
        }

        /// <summary>
        /// 创建降序索引字段。
        /// </summary>
        public static SqlServerIndexColumn Desc(string columnName)
        {
            return new SqlServerIndexColumn
            {
                ColumnName = columnName,
                SortType = SqlServerIndexSortType.Desc
            };
        }

        /// <summary>
        /// 校验字段配置是否合法。
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ColumnName))
                throw new ArgumentException("SQL Server 索引字段名不能为空。");
        }
    }

    /// <summary>
    /// SQL Server 索引定义。
    ///
    /// 设计目标：
    /// 1. 先检查索引是否存在，存在则跳过。
    /// 2. 不存在才创建，避免重复创建报错。
    /// 3. 支持普通索引、唯一索引、过滤索引、包含列索引。
    /// 4. 字段名、表名、索引名统一做安全处理，避免拼接错误。
    ///
    /// 注意：
    /// 1. Filter 是 SQL Server 原生过滤条件，例如：F_DeleteMark IS NULL。
    /// 2. Filter 必须是代码里写死或可信来源，不要直接拼接用户输入。
    /// </summary>
    public class SqlServerIndexDefinition
    {
        /// <summary>
        /// 表名。
        ///
        /// 可以是：
        /// 1. base_merchant_packages
        /// 2. dbo.base_merchant_packages
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 索引名。
        ///
        /// 建议命名规则：
        /// IX_表名_字段1_字段2
        /// UX_表名_字段1_字段2
        /// </summary>
        public string IndexName { get; set; } = string.Empty;

        /// <summary>
        /// 索引字段。
        ///
        /// 顺序很重要：
        /// 1. 等值过滤字段放前面。
        /// 2. 排序字段放后面。
        /// 3. 高频查询字段优先。
        /// </summary>
        public List<SqlServerIndexColumn> Columns { get; set; } = new();

        /// <summary>
        /// 是否唯一索引。
        ///
        /// true:
        /// 数据库强制保证字段组合唯一。
        ///
        /// false:
        /// 只用于提升查询性能。
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        /// 过滤索引条件。
        ///
        /// 示例：
        /// F_DeleteMark IS NULL
        ///
        /// 作用：
        /// 如果大量数据是已删除数据，可以只给未删除数据建索引，减少索引体积。
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// 包含列。
        ///
        /// 适合列表页只展示少量字段时使用。
        /// SQL Server 可以直接从索引中返回这些字段，减少回表。
        ///
        /// 注意：
        /// 1. 不要把大字段放进 IncludeColumns。
        /// 2. 不要把 nvarchar(max) 这种字段放进去。
        /// </summary>
        public List<string> IncludeColumns { get; set; } = new();

        /// <summary>
        /// 检查并创建 SQL Server 索引。
        ///
        /// 返回值：
        /// true  = 本次新建了索引。
        /// false = 索引已经存在，跳过。
        /// </summary>
        public async Task<bool> EnsureAsync(ISqlSugarClient db, CancellationToken cancellationToken = default)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db), "SqlSugar 客户端不能为空。");

            cancellationToken.ThrowIfCancellationRequested();
            Validate();

            if (await ExistsAsync(db))
                return false;

            var sql = BuildCreateSql();
            await db.Ado.ExecuteCommandAsync(sql);

            return true;
        }

        /// <summary>
        /// 校验索引定义。
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(TableName))
                throw new ArgumentException("SQL Server 表名不能为空。");

            if (string.IsNullOrWhiteSpace(IndexName))
                throw new ArgumentException("SQL Server 索引名不能为空。");

            if (Columns == null || Columns.Count == 0)
                throw new ArgumentException($"SQL Server 索引 {IndexName} 至少需要一个索引字段。");

            foreach (var column in Columns)
                column.Validate();
        }

        /// <summary>
        /// 判断索引是否已经存在。
        /// </summary>
        private async Task<bool> ExistsAsync(ISqlSugarClient db)
        {
            var realTableName = TableName.Contains('.')
                ? TableName.Split('.').Last()
                : TableName;

            const string sql = @"
SELECT COUNT(1)
FROM sys.indexes i
INNER JOIN sys.objects o ON i.object_id = o.object_id
WHERE o.type = 'U'
  AND o.name = @TableName
  AND i.name = @IndexName";

            var table = await db.Ado.GetDataTableAsync(
                sql,
                new SugarParameter("@TableName", realTableName),
                new SugarParameter("@IndexName", IndexName));

            if (table == null || table.Rows.Count == 0)
                return false;

            return Convert.ToInt32(table.Rows[0][0]) > 0;
        }

        /// <summary>
        /// 构建 CREATE INDEX SQL。
        /// </summary>
        private string BuildCreateSql()
        {
            var sql = new StringBuilder();

            sql.Append("CREATE ");

            if (Unique)
                sql.Append("UNIQUE ");

            sql.Append("INDEX ");
            sql.Append(QuoteName(IndexName));
            sql.Append(" ON ");
            sql.Append(QuoteTableName(TableName));
            sql.Append(" (");

            sql.Append(string.Join(", ", Columns.Select(x =>
                $"{QuoteName(x.ColumnName)} {(x.SortType == SqlServerIndexSortType.Desc ? "DESC" : "ASC")}")));

            sql.Append(')');

            var includeColumns = IncludeColumns
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (includeColumns.Count > 0)
            {
                sql.Append(" INCLUDE (");
                sql.Append(string.Join(", ", includeColumns.Select(QuoteName)));
                sql.Append(')');
            }

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                sql.Append(" WHERE ");
                sql.Append(Filter);
            }

            return sql.ToString();
        }

        /// <summary>
        /// SQL Server 标识符转义。
        ///
        /// 例如：
        /// F_DeleteMark -> [F_DeleteMark]
        /// </summary>
        private static string QuoteName(string name)
        {
            name = (name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("SQL Server 标识符不能为空。");

            name = name.Trim('[', ']');

            if (name.Contains(']') || name.Contains(';') || name.Contains("--"))
                throw new ArgumentException($"SQL Server 标识符不安全：{name}");

            return $"[{name}]";
        }

        /// <summary>
        /// SQL Server 表名转义。
        ///
        /// 支持：
        /// base_merchant_packages
        /// dbo.base_merchant_packages
        /// </summary>
        private static string QuoteTableName(string tableName)
        {
            var parts = tableName
                .Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(QuoteName);

            return string.Join(".", parts);
        }
    }
}