using SqlSugar;

namespace Shipeng.Util.DataBase
{
    /// <summary>
    /// sqlsugar相关扩展
    /// </summary>
    public static class SqlSugarExtensions
    {
        public static async Task<List<T>> ToPageListAsync<T>(this ISugarQueryable<T> query, SoulPage<T> pagination)
        {
            var tempData = query;
            List<FilterSo> filterSos = pagination.getFilterSos();
            if (filterSos != null && filterSos.Count > 0)
            {
                tempData = tempData.GenerateFilter("a", filterSos);
            }
            if (pagination.order == "desc")
            {
                tempData = tempData.OrderBy(pagination.field + " " + pagination.order);
            }
            else
            {
                tempData = tempData.OrderBy(pagination.field);
            }
            RefAsync<int> totalCount = 0;
            var data = await tempData.ToPageListAsync(pagination.page, pagination.rows, totalCount);
            pagination.count = totalCount;
            return data;
        }
        public static async Task<List<T>> ToPageListAsync<T>(this ISugarQueryable<T> query, Pagination pagination)
        {
            var tempData = query;
            RefAsync<int> totalCount = 0;
            if (pagination.order == "desc")
            {
                tempData = tempData.OrderBy(pagination.field + " " + pagination.order);
            }
            else
            {
                tempData = tempData.OrderBy(pagination.field);
            }
            var data = await tempData.ToPageListAsync(pagination.page, pagination.rows, totalCount);
            pagination.records = totalCount;
            return data;
        }
    }
}
