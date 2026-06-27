using Shipeng.Dependency;
using System;

namespace Shipeng.DynamicApiController
{
    /// <summary>
    /// ¶¯Ì¬ WebApi ÌØÐÔ
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Class)]
    public sealed class DynamicApiControllerAttribute : Attribute
    {
    }
}