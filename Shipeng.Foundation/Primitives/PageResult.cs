namespace Shipeng.Util
{
    /// <summary>
    /// 分页返回结果
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class PageResult<T> : AjaxResult<List<T>>
    {
        /// <summary>
        /// 总记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int Topage { get; set; }

        /// <summary>
        /// 每页多少条
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// 第几页
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 显示的开始
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// 显示的结束
        /// </summary>
        public int End { get; set; }
    }
}