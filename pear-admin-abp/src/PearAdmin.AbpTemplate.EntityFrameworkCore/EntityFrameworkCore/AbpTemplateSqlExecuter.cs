using Abp.Dependency;
using Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Reflection;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.EntityFrameworkCore
{
    /// <summary>
    /// 在ABP中执行原生sql和存储过程
    /// </summary>
    public class AbpTemplateSqlExecuter: IAbpTemplateSqlExecuter, ITransientDependency
    {
        private readonly IDbContextProvider<AbpTemplateDbContext> _dbContextProvider;

        public AbpTemplateSqlExecuter(IDbContextProvider<AbpTemplateDbContext> dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        /// <summary>
        /// 执行给定的命令
        /// </summary>
        /// <param name="sql">命令字符串</param>
        /// <param name="parameters">要应用于命令字符串的参数</param>
        /// <returns>执行命令后由数据库返回的结果</returns>
        public async Task<int> ExecuteAsync(string sql, params object[] parameters)
        {
            return await _dbContextProvider.GetDbContext().Database.ExecuteSqlRawAsync(sql, parameters);
        }

        /// <summary>
        /// 创建一个原始 SQL 查询，该查询将返回给定泛型类型的元素。
        /// </summary>
        /// <typeparam name="T">查询所返回对象的类型</typeparam>
        /// <param name="sql">SQL 查询字符串</param>
        /// <param name="parameters">要应用于 SQL 查询字符串的参数</param>
        /// <returns></returns>
        public IQueryable<T> SqlQuery<T>(string sql, params object[] parameters) where T : class, new()
        {
            var db = _dbContextProvider.GetDbContext().Database;
            return db.SqlQuery<T>(sql, parameters).AsQueryable();
        }

        /// <summary>
        /// 执行命令返回集合
        /// 创建一个原始 SQL 查询，该查询将返回给定泛型类型的元素。
        /// </summary>
        /// <typeparam name="T">查询所返回对象的类型</typeparam>
        /// <param name="sql">SQL 查询字符串</param>
        /// <param name="parameters">要应用于 SQL 查询字符串的参数</param>
        /// <returns></returns>
        public async Task<List<T>> SqlQueryAsync<T>(string sql, params object[] parameters) where T : class, new()
        {
            var db = _dbContextProvider.GetDbContext().Database;
            return await db.SqlQueryAsync<T>(sql, parameters);
        }

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
        public IEnumerable<T> SqlQueryAndParameter<T, D>(string sql, QueryParameter<D> queryParameter,
            List<SqlParameter> param = null) where T : class, new() where D : class, new()
        {
            StringBuilder strsql = new StringBuilder();
            strsql.Append(@"select * from (" + sql + ") t where 1=1");
            if (param == null)
            {
                param = new List<SqlParameter>();
            }
            //var param = new List<SqlParameter>();
            var datadto = queryParameter.data;
            if (datadto != null)
            {
                PropertyInfo[] propertys = datadto.GetType().GetProperties();
                foreach (var item in propertys)
                {
                    var attribute = item.GetCustomAttribute<ConditionsAttribute>();
                    var type = item.PropertyType.FullName;
                    var value = item?.GetValue(datadto);
                    var valueS = Convert.ToString(item?.GetValue(datadto));
                    if (string.IsNullOrWhiteSpace(valueS))
                    {
                        continue;
                    }
                    if (attribute == null)
                    {
                        if (type.Contains("Int"))
                        {
                            if (Convert.ToInt32(value) <= 0)
                            {
                                continue;
                            }
                            param.Add(new SqlParameter("@" + item.Name, value));
                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                        }
                        else if (type.Contains("String"))
                        {
                            param.Add(new SqlParameter("@" + item.Name, "%" + value + "%"));
                            strsql.Append(" and t." + item.Name + " like @" + item.Name);
                        }
                        else if (type.Contains("Datetime"))
                        {
                            var date = ((DateTime)value).ToString("yyyy/MM/dd");
                            param.Add(new SqlParameter("@" + item.Name, date));
                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                        }
                        else
                        {
                            param.Add(new SqlParameter("@" + item.Name, value));
                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                        }
                    }
                    else
                    {
                        if (!attribute.NotSelect)
                        {
                            var conditionsType = attribute.ConditionsTypes;
                            var symbolAttribute = attribute.SymbolAttributes;

                            switch (symbolAttribute)
                            {
                                case SymbolAttribute.EQUAL:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " = @" + item.Name);
                                    break;
                                case SymbolAttribute.CONTAILS:
                                    param.Add(new SqlParameter("@" + item.Name, "%" + value + "%"));
                                    strsql.Append(" and t." + item.Name + " like @" + item.Name);
                                    break;
                                case SymbolAttribute.STARTSWITH:
                                    param.Add(new SqlParameter("@" + item.Name, "%" + value));
                                    strsql.Append(" and t." + item.Name + " like @" + item.Name);
                                    break;
                                case SymbolAttribute.ENDSWITH:
                                    param.Add(new SqlParameter("@" + item.Name, value + "%"));
                                    strsql.Append(" and t." + item.Name + " like @" + item.Name);
                                    break;
                                case SymbolAttribute.GREATER:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " > @" + item.Name);
                                    break;
                                case SymbolAttribute.LESS:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " < @" + item.Name);
                                    break;
                                case SymbolAttribute.GREATEREQUAL:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " >= @" + item.Name);
                                    break;
                                case SymbolAttribute.LESSEQUAL:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " <= @" + item.Name);
                                    break;
                                case SymbolAttribute.INTERVAL:
                                    if (attribute.IsSplit)
                                    {
                                        string[] timestring = valueS.Split(attribute.SplitString);
                                        if (timestring != null)
                                        {
                                            if (timestring.Length == 1)
                                            {
                                                param.Add(new SqlParameter("@" + item.Name + "start", timestring[0]));
                                                strsql.Append(" and (t." + item.Name + " >= @" + item.Name + "start )");
                                            }
                                            else if (timestring.Length == 2)
                                            {
                                                if (timestring[0] != "" && timestring[1] != "")
                                                {
                                                    param.Add(new SqlParameter("@" + item.Name + "start", timestring[0]));
                                                    param.Add(new SqlParameter("@" + item.Name + "end", timestring[1]));
                                                    strsql.Append(" and (t." + item.Name + " >= @" + item.Name + "start and t." + item.Name + " <= @" + item.Name + "end)");
                                                }
                                                else if (timestring[0] != "" && timestring[1] == "")
                                                {
                                                    param.Add(new SqlParameter("@" + item.Name + "start", timestring[0]));
                                                    strsql.Append(" and (t." + item.Name + " >= @" + item.Name + "start )");
                                                }
                                                else if (timestring[0] == "" && timestring[1] != "")
                                                {
                                                    param.Add(new SqlParameter("@" + item.Name + "start", timestring[1]));
                                                    strsql.Append(" and ( t." + item.Name + " <= @" + item.Name + "end)");
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    switch (conditionsType)
                                    {
                                        case ConditionsType.INT:
                                            param.Add(new SqlParameter("@" + item.Name, value));
                                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                                            break;
                                        case ConditionsType.STRING:
                                            param.Add(new SqlParameter("@" + item.Name, "%" + value + "%"));
                                            strsql.Append(" and t." + item.Name + " like @" + item.Name);
                                            break;
                                        case ConditionsType.DATETIME:
                                            var date = ((DateTime)value).ToString("yyyy/MM/dd");
                                            param.Add(new SqlParameter("@" + item.Name, date));
                                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                                            break;
                                        default:
                                            param.Add(new SqlParameter("@" + item.Name, value));
                                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                                            break;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            queryParameter.Count = _dbContextProvider.GetDbContext().Set<T>().FromSqlRaw(strsql.ToString(), param.ToArray()).Count();
            if (queryParameter.sidx == "")
            {
                queryParameter.sidx = "Id";
                queryParameter.sord = "desc";
            }
            strsql.Append($@" order by {queryParameter.sidx} {queryParameter.sord} offset {(queryParameter.pageNum - 1) * queryParameter.pageSize} rows fetch next {queryParameter.pageSize} rows only");
            return SqlQueryForParameter<T>(strsql.ToString(), param.ToArray());
        }
       
        public IEnumerable<T> SqlQueryForParameter<T>(string sql, params object[] parameters) where T : class, new()
        {
            return _dbContextProvider.GetDbContext().Set<T>().FromSqlRaw(sql, parameters);
        }
        #endregion

    }
}
