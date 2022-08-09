using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace Shipeng.Domain.Entities
{
    /// <summary>
    /// 网关集群部署节点信息表
    /// </summary>
    public class Node : Entity<Guid>
    {
        public Node() { }
        public Node(Guid id) : base(id)
        {

        }
        /// <summary>
        /// 节点名称
        /// </summary>
        [MaxLength(128)]
        public string NodeName { get; set; }
        /// <summary>
        /// 节点描述
        /// </summary>
        [MaxLength(512)]
        public string Description { get; set; }
        /// <summary>
        /// 节点服务端地址
        /// </summary>
        [MaxLength(1024)]
        public string Server { get; set; }
        /// <summary>
        /// 访问Token
        /// </summary>
        [MaxLength(512)]
        public string AccessToken { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? Updated { get; set; }
    }
}
