using System.ComponentModel.DataAnnotations;

namespace Shipeng.Domain.Shared.Enums
{
    /// <summary>
    /// 白名单过滤类型
    /// </summary>
    public enum FilterTypeEnum
    {
        /// <summary>
        /// 路径
        /// </summary>
        [Display(Name = "路径")]
        Path = 0,
        /// <summary>
        /// 正则
        /// </summary>
        [Display(Name = "正则")]
        Regular = 1
    }
}
