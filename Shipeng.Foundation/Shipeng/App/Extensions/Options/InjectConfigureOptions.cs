using Shipeng.SpecificationDocument;
using System;

namespace Shipeng
{
    /// <summary>
    /// Inject 中间件配置选项
    /// </summary>
    public sealed class InjectConfigureOptions
    {
        /// <summary>
        /// 规范化结果中间件配置
        /// </summary>
        public Action<SpecificationDocumentConfigureOptions> SpecificationDocumentConfigure { get; set; }
    }
}