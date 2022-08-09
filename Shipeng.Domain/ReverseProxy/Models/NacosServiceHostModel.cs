namespace Shipeng.Domain.ReverseProxy.Models
{
    /// <summary>
    /// Nacos服务主机模型
    /// </summary>
    public class NacosServiceHostModel
    {
        /// <summary>
        /// 实例ID
        /// </summary>
        public string InstanceId { get; set; }
        /// <summary>
        /// ip
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Weight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Healthy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Enabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Ephemeral { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClusterName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int InstanceHeartBeatInterval { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InstanceIdGenerator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int InstanceHeartBeatTimeOut { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int IpDeleteTimeout { get; set; }
    }
}
