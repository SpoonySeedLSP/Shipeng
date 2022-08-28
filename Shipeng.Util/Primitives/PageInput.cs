namespace Shipeng.Util
{
    /// <summary>
    /// 分页查询基类
    /// </summary>
    public class PageInput
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页行数
        /// </summary>
        public int PageRows { get; set; } = int.MaxValue;

        /// <summary>
        /// 排序列
        /// </summary>
        public string SortField { get; set; } = "Id";

        /// <summary>
        /// 排序类型
        /// </summary>
       // public string SortType { get => _sortType; set => _sortType = (value ?? string.Empty).ToLower().Contains("desc") ? "desc" : "asc"; }
        public string SortType { get; set; } = "asc";
    }

    /// <summary>
    /// 获取固定数量的list
    /// </summary>
    public class StripInput
    {
        /// <summary>
        /// 获取多少条
        /// </summary>
        public int Count { get; set; } = int.MaxValue;

        /// <summary>
        /// 排序列
        /// </summary>
        public string SortField { get; set; } = "Id";

        /// <summary>
        /// 排序类型
        /// </summary>
        // public string SortType { get => _sortType; set => _sortType = (value ?? string.Empty).ToLower().Contains("desc") ? "desc" : "asc"; }

        public string SortType { get; set; } = "asc";
    }

    /// <summary>
    /// 要修改的字段
    /// </summary>
    public class UpdateData
    {
        /// <summary>
        /// 要修改的字段
        /// </summary>
        public List<string> Properties { get; set; }
    }

    /// <summary>
    /// 导出
    /// </summary>
    public class ExportData
    {
        /// <summary>
        /// 导出类型 全部数据=0,按搜索条件导出全部=1,导出选择数据=2
        /// </summary>
        public ExportTypes ExportType { get; set; }

        /// <summary>
        /// 需要的字段
        /// </summary>
        public List<ExportField> ExportFieldList { get; set; }
    }

    /// <summary>
    /// 可选导出字段
    /// </summary>
    public class ExportField
    {
        /// <summary>
        /// 是否必选
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// 字段
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
    }
}