using Shipeng.Dependency;
using System;

namespace Shipeng.InstantMessaging
{
    /// <summary>
    /// 即时通信集线器配置特�?
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Class)]
    public sealed class MapHubAttribute : Attribute
    {
        /// <summary>
        /// 构造函�?
        /// </summary>
        /// <param name="pattern"></param>
        public MapHubAttribute(string pattern)
        {
            Pattern = pattern;
        }

        /// <summary>
        /// 配置终点路由地址
        /// </summary>
        public string Pattern { get; set; }
    }
}