using Shipeng.Dependency;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Ìø¹ıÑéÖ¤
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class NonValidationAttribute : Attribute
    {
    }
}