using System.ComponentModel;

namespace Shipeng.Util
{
    /// <summary>
    /// 性别枚举
    /// </summary>
    public enum Sex
    {
        [Description("男")]
        Man = 1,

        [Description("女")]
        Woman = 2,

        [Description("未知")]
        Unknown = 0
    }
}
