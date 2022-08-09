namespace Shipeng.Application.Contracts.Dtos.ReverseProxy
{
    public class RouteMainDto
    {
        /// <summary>
        /// 路由ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 路由名称
        /// </summary>
        public string RouteName { get; set; }
        /// <summary>
        /// 服务状态(0.关闭 1.开启)
        /// </summary>
        public bool UseState { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 路由路径规则
        /// </summary>
        public string RouteMatchPath { get; set; }
    }
}
