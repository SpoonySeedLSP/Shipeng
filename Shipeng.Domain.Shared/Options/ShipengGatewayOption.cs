namespace Shipeng.Domain.Shared.Options
{
    /// <summary>
    /// 网关配置项
    /// </summary>
    public class ShipengGatewayOption
    {
        /// <summary>
        /// 网关管理端服务器地址
        /// </summary>
        public string AdminServer { get; set; }
        /// <summary>
        /// 配置数据热更新访问授权token
        /// </summary>
        public string AccessToken { get; set; }
    }
}
