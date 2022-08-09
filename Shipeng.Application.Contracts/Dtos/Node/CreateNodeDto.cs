using System.ComponentModel.DataAnnotations;

namespace Shipeng.Application.Contracts.Dtos.Node
{
    public class CreateNodeDto
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        [Required]
        public string NodeName { get; set; }
        /// <summary>
        /// 节点描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 节点服务端地址
        /// </summary>
        [Required]
        public string Server { get; set; }
        /// <summary>
        /// 访问Token
        /// </summary>
        [Required]
        public string AccessToken { get; set; }
    }
}
