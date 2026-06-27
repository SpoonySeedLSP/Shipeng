using Shipeng.Dependency;

namespace Shipeng.EntityFrameworkCore
{
    /// <summary>
    /// 匹配类型
    /// </summary>
    [SuppressSniffer]
    public enum MatchType
    {
        /// <summary>
        /// 任意一个
        /// </summary>
        Any = 0,

        /// <summary>
        /// 全部匹配
        /// </summary>
        All = 1
    }
}
