using Shipeng.Dependency;
using System;

namespace Shipeng.RemoteRequest
{
    /// <summary>
    /// 代理参数基类特性
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterBaseAttribute : Attribute
    {
    }
}