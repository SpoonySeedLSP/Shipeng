namespace Shipeng.Application.Contracts
{
    /// <summary>
    /// 通用分页结模型
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class ShipengPageResult<TResult> : ShipengResult<TResult>
    {
        /// <summary>
        /// 分页获取数据时有作用
        /// </summary>
        public int Count { get; set; }
    }
}
