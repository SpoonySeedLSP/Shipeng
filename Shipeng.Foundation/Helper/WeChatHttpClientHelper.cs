using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Shipeng.Util
{
    /// <summary>
    /// 微信支付 V2 类接口的 HTTP 请求辅助类。
    /// </summary>
    /// <remarks>
    /// 微信支付、企业付款、红包等旧接口通常以 XML 作为请求体，同时将 Content-Type
    /// 设置为 application/x-www-form-urlencoded；退款、红包、企业付款等接口还要求
    /// 通过商户 PFX 证书完成双向 TLS 认证。把这段逻辑集中在一个内部类里，可以避免
    /// 每个业务 Helper 重复维护连接、证书和响应读取逻辑。
    /// </remarks>
    internal static class WeChatHttpClientHelper
    {
        private const string FormContentType = "application/x-www-form-urlencoded";

        /// <summary>
        /// 发送微信 V2 XML POST 请求。
        /// </summary>
        /// <param name="url">微信接口地址。</param>
        /// <param name="xml">已经完成签名的 XML 请求体。</param>
        /// <param name="certificate">商户 PFX 证书；普通统一下单等接口不需要证书时传 null。</param>
        /// <returns>微信接口返回的 XML 字符串。</returns>
        public static string PostXml(string url, string xml, X509Certificate2 certificate = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("微信接口地址不能为空。", nameof(url));
            }

            using HttpClientHandler handler = CreateHandler(certificate);
            using HttpClient client = new HttpClient(handler, disposeHandler: true);
            using var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url)
            {
                Content = new StringContent(xml ?? string.Empty, Encoding.UTF8, FormContentType)
            };

            using HttpResponseMessage response = client.Send(request);
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private static HttpClientHandler CreateHandler(X509Certificate2 certificate)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            if (certificate != null)
            {
                handler.ClientCertificates.Add(certificate);
            }

            return handler;
        }
    }
}
