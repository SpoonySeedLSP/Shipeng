using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace Shipeng.Domain.Entities
{
    /// <summary>
    /// 集群目的地信息表
    /// </summary>
    public class ClusterDestination : Entity<Guid>
    {
        public ClusterDestination() { }
        public ClusterDestination(Guid id) : base(id)
        {
        }
        /// <summary>
        /// 所属集群ID
        /// </summary>
        public Guid ClusterId { get; set; }
        /// <summary>
        /// 目的地名称
        /// </summary>
        [MaxLength(128)]
        public string DestinationName { get; set; }
        /// <summary>
        /// 目的地地址
        /// </summary>
        [MaxLength(1024)]
        public string DestinationAddress { get; set; }
    }
}
