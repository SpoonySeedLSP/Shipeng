using System.Data.Common;

namespace Shipeng.EntityFrameworkCore
{
    /// <summary>
    /// FormattableString扩展类
    /// </summary>
    public static class FormattableStringExtensions
    {
        /// <summary>
        /// sql参数化
        /// </summary>
        /// <param name="sql">内插sql字符串</param>
        /// <param name="databaseType">数据库类型</param>
        /// <returns></returns>
        public static (string sqlFormat, DbParameter[] parameters) ToDbParameter(this FormattableString sql, DatabaseType databaseType)
        {
            var (sqlFormat, parameter) = sql.ToParameter(databaseType);

            return (sqlFormat, databaseType switch
            {
                DatabaseType.SqlServer => parameter.ToSqlParameters(),
                DatabaseType.MySql => parameter.ToMySqlParameters(),
                DatabaseType.Sqlite => parameter.ToSqliteParameters(),
                DatabaseType.Oracle => parameter.ToOracleParameters(),
                DatabaseType.PostgreSql => parameter.ToNpgsqlParameters(),
                _ => null
            });
        }

        /// <summary>
        /// sql参数化
        /// </summary>
        /// <param name="sql">内插sql字符串</param>
        /// <param name="databaseType">数据库类型</param>
        /// <returns></returns>
        public static (string sqlFormat, Dictionary<string, object> parameter) ToParameter(this FormattableString sql, DatabaseType databaseType)
        {
            if (sql == null)
                throw new ArgumentNullException(nameof(sql));

            var sqlFormat = sql.Format;
            var parameter = new Dictionary<string, object>();
            var arguments = sql.GetArguments();

            if (sql.ArgumentCount <= 0)
                return (sqlFormat, parameter);

            var prefix = databaseType switch
            {
                DatabaseType.Sqlite => "@",
                DatabaseType.SqlServer => "@",
                DatabaseType.MySql => "?",
                DatabaseType.Oracle => ":",
                DatabaseType.PostgreSql => ":",
                _ => "",
            };

            for (int i = 0; i < sql.ArgumentCount; i++)
            {
                var pName = $"{prefix}p__{i + 1}";

                sqlFormat = sqlFormat.Replace($"{{{i}}}", pName);

                parameter[pName] = arguments[i];
            }

            return (sqlFormat, parameter);
        }
    }
}
