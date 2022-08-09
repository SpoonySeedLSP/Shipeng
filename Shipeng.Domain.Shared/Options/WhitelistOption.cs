namespace Shipeng.Domain.Shared.Options
{
    /// <summary>
    /// 白名单配置项
    /// </summary>
    public class WhitelistOption
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 所属路由(为空则全局)
        /// </summary>
        public string RouteId { get; set; }
        /// <summary>
        /// 白名单名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 过滤文本(根据类型保存的值为路径或者正则表达式)
        /// </summary>
        public string FilterText { get; set; }
        /// <summary>
        /// 请求方式(多请求方式使用,分隔)
        /// </summary>
        public string RequestMethod { get; set; }

    }
}
