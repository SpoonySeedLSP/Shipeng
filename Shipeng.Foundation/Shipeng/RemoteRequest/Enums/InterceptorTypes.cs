using Shipeng.Dependency;
using System.ComponentModel;

namespace Shipeng.RemoteRequest
{
    /// <summary>
    /// ¿πΩÿ¿‡–Õ
    /// </summary>
    [SuppressSniffer]
    public enum InterceptorTypes
    {
        /// <summary>
        /// HttpClient ¿πΩÿ
        /// </summary>
        [Description("HttpClient ¿πΩÿ")]
        Client,

        /// <summary>
        /// HttpRequestMessage ¿πΩÿ
        /// </summary>
        [Description("HttpRequestMessage ¿πΩÿ")]
        Request,

        /// <summary>
        /// HttpResponseMessage ¿πΩÿ
        /// </summary>
        [Description("HttpResponseMessage ¿πΩÿ")]
        Response,

        /// <summary>
        /// “Ï≥£¿πΩÿ
        /// </summary>
        [Description("“Ï≥£¿πΩÿ")]
        Exception
    }
}