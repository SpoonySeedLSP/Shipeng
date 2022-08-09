using System.ComponentModel;

namespace Shipeng.Util
{
    public enum ExportTypes
    {
        /// <summary>
        /// 全部数据
        /// </summary>
        [Description("全部数据")]
        All = 0,
        /// <summary>
        /// 按搜索条件导出全部
        /// </summary>
        [Description("按搜索条件导出全部")]
        Search = 1,
        /// <summary>
        /// 导出选择数据
        /// </summary>
        [Description("导出选择数据")]
        Select = 2
    }
}
