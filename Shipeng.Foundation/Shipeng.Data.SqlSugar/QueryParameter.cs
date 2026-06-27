namespace SqlSugar
{
    /// <summary>
    /// 用来接收分页排序和查询参数
    /// 这个类可以自定义，但是要保证查询参数和数据库的字段名一致，
    /// 和后面的SqlQueryAndParameter中的接收参数一致，
    /// 最好也和前端的传到接口中的数据格式一致，这样就不需要在进行其他处理，直接进行查询就行了
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryParameter<T> where T : class
    {
        /// <summary>
        /// 查询参数
        /// </summary>
        public T data { get; set; } 

        /// <summary>
        /// 当前页
        /// </summary>
        public int pageNum { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int pageSize { get; set; } = 10;

        /// <summary>
        /// 总条数
        /// </summary>
        public int Count { get; set; } = 0;

        /// <summary>
        /// 排序列
        /// </summary>
        public string sidx { get; set; } = "Id";

        /// <summary>
        /// 排序类型
        /// </summary>
        public string sord { get; set; } = "desc";
    }
}
