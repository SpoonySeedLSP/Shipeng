using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Shipeng.EntityFrameworkCore
{
    /// <summary>
    /// MySql仓储实现类
    /// </summary>
    public class MySqlRepository : BaseRepository
    {
        #region Property
        /// <summary>
        /// 数据库类型
        /// </summary>
        public override DatabaseType DatabaseType => DatabaseType.MySql;
        #endregion

        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        public MySqlRepository() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">DbContext实例</param>
        public MySqlRepository(DbContext context) : base(context) { }
        #endregion

        #region Page
        /// <summary>
        /// 获取分页语句
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">原始sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序排序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns></returns>
        public override string GetPageSql(bool isWithSyntax, string sql, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            //排序字段
            if (!orderField.IsNullOrEmpty())
            {
                if (orderField.Contains(@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)", RegexOptions.IgnoreCase))
                    orderField = $"ORDER BY {orderField}";
                else
                    orderField = $"ORDER BY {orderField} {(isAscending ? "ASC" : "DESC")}";
            }

            string sqlQuery;
            var limit = pageSize;
            var offset = pageSize * (pageIndex - 1);

            //判断是否with语法
            if (isWithSyntax)
            {
                sqlQuery = $"{sql} SELECT {CountSyntax} AS `TOTAL` FROM T;";

                sqlQuery += $"{sql.Remove(sql.LastIndexOf(")"), 1)} {orderField}) SELECT * FROM T LIMIT {limit} OFFSET {offset};";
            }
            else
            {
                sqlQuery = $"SELECT {CountSyntax} AS `TOTAL` FROM ({sql}) AS T;";

                sqlQuery += $"{sql} {orderField} LIMIT {limit} OFFSET {offset};";
            }

            return sqlQuery;
        }
        #endregion
    }
}
