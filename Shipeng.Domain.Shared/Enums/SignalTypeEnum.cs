using System.ComponentModel.DataAnnotations;

namespace Shipeng.Domain.Shared.Enums
{
    /// <summary>
    /// 通信方式
    /// </summary>
    public enum SignalTypeEnum
    {
        /// <summary>
        /// Http(仅支持post)
        /// </summary>
        [Display(Name = "Http(仅支持POST请求)")]
        Http = 0,
        /// <summary>
        /// GRPC
        /// </summary>
        [Display(Name = "GRPC(暂时不支持)")]
        Grpc = 1
    }
}
