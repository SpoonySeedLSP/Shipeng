namespace Shipeng.Application.Contracts.Dtos.ReverseProxy
{
    public class RouteTransformDto
    {
        /// <summary>
        /// 交换配置ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 关联服务ID
        /// </summary>
        public Guid RouteId { get; set; }
        /// <summary>
        /// 交换配置项名称
        /// </summary>
        public string TransformsName { get; set; }
        /// <summary>
        /// 交换配置项值
        /// </summary>
        public string TransformsValue { get; set; }
    }
}
