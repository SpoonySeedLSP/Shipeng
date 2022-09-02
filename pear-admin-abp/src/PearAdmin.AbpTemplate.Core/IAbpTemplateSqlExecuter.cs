using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate
{
    /// <summary>
    /// 在ABP中执行原生sql和存储过程
    /// 有些查询(或存储过程)直接使用sql语句会更方便一点
    /// </summary>
    public interface IAbpTemplateSqlExecuter
    {
        /// <summary>
        /// 执行给定的命令
        /// </summary>
        /// <param name="sql">命令字符串</param>
        /// <param name="parameters">要应用于命令字符串的参数</param>
        /// <returns>执行命令后由数据库返回的结果</returns>
        Task<int> ExecuteAsync(string sql, params object[] parameters);

        /// <summary>
        /// 创建一个原始 SQL 查询，该查询将返回给定泛型类型的元素。
        /// </summary>
        /// <typeparam name="T">查询所返回对象的类型</typeparam>
        /// <param name="sql">SQL 查询字符串</param>
        /// <param name="parameters">要应用于 SQL 查询字符串的参数</param>
        /// <returns></returns>
        IQueryable<T> SqlQuery<T>(string sql, params object[] parameters) where T : class, new();

        /// <summary>
        /// 执行命令返回集合
        /// 创建一个原始 SQL 查询，该查询将返回给定泛型类型的元素。
        /// </summary>  
        /// <typeparam name="T">查询所返回对象的类型</typeparam>
        /// <param name="sql">SQL 查询字符串</param>
        /// <param name="parameters">要应用于 SQL 查询字符串的参数</param>
        /// <returns></returns>
        Task<List<T>> SqlQueryAsync<T>(string sql, params object[] parameters) where T : class, new();

        #region 拼接sql字符串
        /// <summary>
        /// 在查询条件的DTO中给字段加特性ConditionsAttribute
        /// 没有ConditionsAttribute特性时，会根据字段自身的类型进行查询，int时按=，string时按like，datetime时搜索当天
        /// 有特性时 跟根据特性一定的特性来进行搜索
        /// NotSelect为永不查询，即使DTO中有值也不会查询
        /// Enable是否启用，如果不启用的话，同没有特性的逻辑（已废弃）
        /// symbolAttribute是判断字符运算符，根据运算符进行查询，目前有=，>,<,>=,<=,like，范围等，详情请看SymbolAttribute枚举
        /// IsSplit为是否有分隔符，只有在运算符为范围时生效
        /// SplitString是分隔符的字符，只有在IsSplit为true时生效
        /// 另外，此方法还支持自定义写查询条件，可以在param中定义不在DTO中的查询条件，写在sql字符串中即可
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="sql"></param>
        /// <param name="queryParameter"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        IEnumerable<T> SqlQueryAndParameter<T, D>(string sql, QueryParameter<D> queryParameter,
            List<SqlParameter> param = null) where T : class, new() where D : class, new();

        public IEnumerable<T> SqlQueryForParameter<T>(string sql, params object[] parameters) where T : class, new();
        #endregion

    }
}
