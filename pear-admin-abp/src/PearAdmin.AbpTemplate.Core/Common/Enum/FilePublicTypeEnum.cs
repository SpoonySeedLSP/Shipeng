using System.ComponentModel;

namespace PearAdmin.AbpTemplate.Common.Enum
{
    /// <summary>
    /// 文件公开类型枚举
    /// </summary>
    public enum FilePublicTypeEnum
    {
        /// <summary>
        /// 完全公开
        /// </summary>
        [Description("完全公开")]
        FullyPublic = 0,
        /// <summary>
        /// 部门内部公开
        /// </summary>
        [Description("部门内部公开")]
        Public = 1,
        /// <summary>
        /// 仅自己可见
        /// </summary>
        [Description("仅自己可见")]
        Private = 2
    }
}
