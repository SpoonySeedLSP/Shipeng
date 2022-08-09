namespace Shipeng.Application.Contracts
{
    /// <summary>
    /// 通用返回结果模型
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class ShipengResult<TResult>
    {
        /// <summary>
        /// 结果码(为0则表示请求成功)
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 结果数据
        /// </summary>
        public TResult? Data { get; set; }
    }

    public class ShipengResult
    {
        /// <summary>
        /// 结果码(为0则表示请求成功)
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string? Message { get; set; }
    }

}
