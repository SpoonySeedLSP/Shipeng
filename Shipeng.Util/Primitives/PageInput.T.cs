namespace Shipeng.Util
{
    /// <summary>
    /// 分页
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class PageInput<T> : PageInput where T : new()
    {
        public T Search { get; set; } = new T();
    }

    /// <summary>
    /// 固定条数
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class StripInput<T> : StripInput where T : new()
    {
        public T Search { get; set; } = new T();
    }

    /// <summary>
    /// 修改固定字段
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class UpdateData<T> : UpdateData where T : new()
    {
        public T Search { get; set; } = new T();
    }

    /// <summary>
    /// 导出
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class ExportData<T> : ExportData where T : new()
    {
        /// <summary>
        /// 查询关键字
        /// </summary>
        public T Search { get; set; } = new T();

        /// <summary>
        /// 选择将要导出的id列表
        /// </summary>
        public List<string> ids { get; set; } = null;
    }
}