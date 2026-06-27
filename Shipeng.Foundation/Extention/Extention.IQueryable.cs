using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Shipeng.Util
{
    /// <summary>
    /// IQueryable"T"的拓展操作
    /// </summary>
    public static partial class Extention
    {
        /// <summary>
        /// 符合条件则Where
        /// </summary>
        /// <typeparam name="T"> 实体类型 </typeparam>
        /// <param name="q"> 数据源 </param>
        /// <param name="need"> 是否符合条件 </param>
        /// <param name="where"> 筛选 </param>
        /// <returns> </returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> q, bool need, Expression<Func<T, bool>> where)
        {
            if (need)
            {
                return q.Where(where);
            }
            else
            {
                return q;
            }
        }

        /// <summary>
        /// 动态排序法
        /// </summary>
        /// <typeparam name="T"> 实体类型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="sort"> 排序规则，Key为排序列，Value为排序类型 </param>
        /// <returns> </returns>
        private static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, List<KeyValuePair<string, string>> sort)
        {
            PropertyInfo property = null;
            if (sort.Count > 1)
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T), "o");
                string OrderName = "";

                sort.ForEach((aSort, index) =>
                {
                    //根据属性名获取属性
                    property = GetTheProperty(typeof(T), aSort.Key);
                    if (!property.IsNullOrEmpty())
                    {
                        if (index > 0)
                        {
                            OrderName = aSort.Value.ToLower() == "desc" ? "ThenByDescending" : "ThenBy";
                        }
                        else
                        {
                            OrderName = aSort.Value.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
                        }
                        source = source.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), OrderName, new Type[] { typeof(T), property.PropertyType }, source.Expression, Expression.Quote(Expression.Lambda(Expression.MakeMemberAccess(parameter, property), parameter))));
                    }
                });
            }
            else
            {
                property = GetTheProperty(typeof(T), sort[0].Key);
                if (!property.IsNullOrEmpty())
                {
                    source = source.OrderBy($@"{sort[0].Key} {sort[0].Value}");
                }
            }
            return (IOrderedQueryable<T>)source;
        }

        /// <summary>
        ///  //必须追溯到最基类属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static PropertyInfo GetTheProperty(Type type, string propertyName)
        {
            //if (type.BaseType.GetProperties().Any(x => x.Name == propertyName))
            //{
            //    return GetTheProperty(type.BaseType, propertyName);
            //}
            //else
            //{
            //    return type.GetProperty(propertyName);
            //}
            if (type.GetProperties().Any(x => x.Name == propertyName))//判断是否包含 Deleted字段
            {
                return type.GetProperty(propertyName);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// List string 合并成 List KeyValuePair
        /// </summary>
        /// <param name="fieidIng"> </param>
        /// <param name="SortTypeIng"> </param>
        /// <returns> </returns>
        private static List<KeyValuePair<string, string>> ToListStringKeyValuePair(string fieidIng, string SortTypeIng)
        {
            List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();

            if (!fieidIng.IsNullOrEmpty())
            {
                List<string> fieid = fieidIng.ToStringList().ToDistinctList();
                if (fieid.Count > 1)
                {
                    List<string> SortType = SortTypeIng.ToStringList();

                    for (int i = 0; i < fieid.Count; i++)
                    {
                        if (SortType.Count > i)
                        {
                            keyValuePairs.Add(new KeyValuePair<string, string>(fieid[i], SortType[i].ToLower().Contains("desc") ? "desc" : "asc"));
                        }
                        else
                        {
                            keyValuePairs.Add(new KeyValuePair<string, string>(fieid[i], "asc"));
                        }
                    }
                }
                else
                {
                    keyValuePairs.Add(new KeyValuePair<string, string>(fieidIng, SortTypeIng.ToLower().Contains("desc") ? "desc" : "asc"));
                }
            }
            else
            {
                keyValuePairs.Add(new KeyValuePair<string, string>("Id", "asc"));
            }

            return keyValuePairs;
        }

        /// <summary>
        /// 获取有多少页
        /// </summary>
        /// <param name="count">总数</param>
        /// <param name="PageRows">没页多少条</param>
        /// <returns></returns>
        private static int GetPages(int count, int PageRows)
        {
            int pages = count / PageRows;
            pages = count % PageRows == 0 ? pages : pages + 1;
            return pages;
        }

        /// <summary>
        /// 获取开始
        /// </summary>
        /// <param name="PageIndex">第几页</param>
        /// <param name="PageRows">没页多少条</param>
        /// <returns></returns>
        private static int GetStart(int PageIndex, int PageRows)
        {
            return (PageIndex - 1) * PageRows + 1;
        }

        /// <summary>
        /// 获取结束
        /// </summary>
        /// <param name="count">总数量</param>
        /// <param name="PageIndex">第几页</param>
        /// <param name="PageRows">没页多少条</param>
        /// <returns></returns>
        private static int GetEnd(int count, int PageIndex, int PageRows)
        {
            int end = PageIndex * PageRows;
            if (end > count)
            {
                end = count;
            }
            return end;
        }

        /// <summary>
        /// 构造返回值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <param name="pageInput"></param>
        /// <returns></returns>
        private static PageResult<T> GetReturn<T>(List<T> list, int count, PageInput pageInput)
        {
            return new PageResult<T> { Data = list, Topage = GetPages(count, pageInput.PageRows), Total = count, Page = pageInput.PageIndex, Rows = pageInput.PageRows, Start = GetStart(pageInput.PageIndex, pageInput.PageRows), End = GetEnd(count, pageInput.PageIndex, pageInput.PageRows) };
        }

        /// <summary>
        /// 返回分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ob"></param>
        /// <param name="pageInput"></param>
        /// <returns></returns>
        private static IQueryable<T> GetSkipTake<T>(this IOrderedQueryable<T> ob, PageInput pageInput)
        {
            return ob.Skip((pageInput.PageIndex - 1) * pageInput.PageRows).Take(pageInput.PageRows);
        }

        #region 分页数据(包括总数量)

        /// <summary>
        /// IEnumerable 获取分页数据(包括总数量)
        /// </summary>
        /// <typeparam name="T"> IEnumerable泛型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="pageInput"> 分页参数 </param>
        /// <returns> </returns>
        public static PageResult<T> GetPageResult<T>(this IEnumerable<T> source, PageInput pageInput)
        {
            int count = source.Count();
            IOrderedQueryable<T> ob = source.AsQueryable().OrderBy(ToListStringKeyValuePair(pageInput.SortField, pageInput.SortType));
            List<T> list = ob.GetSkipTake(pageInput).ToList();
            return GetReturn<T>(list, count, pageInput);
        }

        /// <summary>
        /// 获取分页数据(包括总数量)
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="pageInput"> 分页参数 </param>
        /// <returns> </returns>
        public static PageResult<T> GetPageResult<T>(this IQueryable<T> source, PageInput pageInput)
        {
            int count = source.Count();
            IOrderedQueryable<T> ob = source.AsQueryable().OrderBy(ToListStringKeyValuePair(pageInput.SortField, pageInput.SortType));
            List<T> list = ob.GetSkipTake(pageInput).ToList();
            return GetReturn<T>(list, count, pageInput);
        }

        /// <summary>
        /// 获取分页数据(包括总数量)
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="pageInput"> 分页参数 </param>
        /// <returns> </returns>
        public static async Task<PageResult<T>> GetPageResultAsync<T>(this IQueryable<T> source, PageInput pageInput)
        {
            int count = await source.CountAsync();
            IOrderedQueryable<T> ob = source.OrderBy(ToListStringKeyValuePair(pageInput.SortField, pageInput.SortType));
            List<T> list = await ob.GetSkipTake(pageInput).ToListAsync();
            return GetReturn<T>(list, count, pageInput);
        }

        #endregion 分页数据(包括总数量)

        #region 分页,不获取总数量

        /// <summary>
        /// 获取分页数据(仅获取列表,不获取总数量)
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="pageInput"> 分页参数 </param>
        /// <returns> </returns>
        public static List<T> GetPageList<T>(this IQueryable<T> source, PageInput pageInput)
        {
            IOrderedQueryable<T> ob = source.OrderBy(ToListStringKeyValuePair(pageInput.SortField, pageInput.SortType));
            List<T> list = ob.GetSkipTake(pageInput).ToList();
            return list;
        }

        /// <summary>
        /// 获取分页数据(仅获取列表,不获取总数量)
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="pageInput"> 分页参数 </param>
        /// <returns> </returns>
        public static async Task<List<T>> GetPageListAsync<T>(this IQueryable<T> source, PageInput pageInput)
        {
            IOrderedQueryable<T> ob = source.OrderBy(ToListStringKeyValuePair(pageInput.SortField, pageInput.SortType));
            List<T> list = await ob.GetSkipTake(pageInput).ToListAsync();
            return list;
        }

        #endregion 分页,不获取总数量

        #region 固定数量

        /// <summary>
        /// 获取固定条数的数据
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="SortField"> 排序字段 </param>
        /// <param name="SortTypes"> 排序方式 </param>
        /// <param name="Count"> 数量 </param>
        /// <returns> </returns>
        public static List<T> GetCountList<T>(this IQueryable<T> source, string SortField, string SortTypes, int Count)
        {
            IOrderedQueryable<T> ob = source.OrderBy(ToListStringKeyValuePair(SortField, SortTypes));
            List<T> list;
            if (Count == int.MaxValue)
            {
                list = ob.ToList();
            }
            else
            {
                list = ob.Take(Count).ToList();
            }
            return list;
        }

        /// <summary>
        /// 获取固定条数的数据
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="stripInput"> 排序数据 </param>
        /// <returns> </returns>
        public static List<T> GetCountList<T>(this IQueryable<T> source, StripInput stripInput)
        {
            IOrderedQueryable<T> ob = source.OrderBy(ToListStringKeyValuePair(stripInput.SortField, stripInput.SortType));
            List<T> list;
            if (stripInput.Count == int.MaxValue)
            {
                list = ob.ToList();
            }
            else
            {
                list = ob.Take(stripInput.Count).ToList();
            }
            return list;
        }

        /// <summary>
        /// 异步 获取固定条数的数据
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="SortField"> 排序字段 </param>
        /// <param name="SortTypes"> 排序方式 </param>
        /// <param name="Count"> 数量 </param>
        /// <returns> </returns>
        public static async Task<List<T>> GetCountListAsync<T>(this IQueryable<T> source, string SortField, string SortTypes, int Count)
        {
            IOrderedQueryable<T> ob = source.OrderBy(ToListStringKeyValuePair(SortField, SortTypes));
            List<T> list = await ob.Take(Count).ToListAsync();
            return list;
        }

        /// <summary>
        /// 异步 获取固定条数的数据
        /// </summary>
        /// <typeparam name="T"> 泛型 </typeparam>
        /// <param name="source"> 数据源 </param>
        /// <param name="stripInput"> 排序数据 </param>
        /// <returns> </returns>
        public static async Task<List<T>> GetCountListAsync<T>(this IQueryable<T> source, StripInput stripInput)
        {
            IOrderedQueryable<T> ob = source.OrderBy(ToListStringKeyValuePair(stripInput.SortField, stripInput.SortType));
            List<T> list;
            if (stripInput.Count == int.MaxValue)
            {
                list = await ob.ToListAsync();
            }
            else
            {
                list = await ob.Take(stripInput.Count).ToListAsync();
            }
            return list;
        }

        #endregion 固定数量
    }
}