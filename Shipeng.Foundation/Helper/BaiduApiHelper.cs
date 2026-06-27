using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Shipeng.Util
{
    /// <summary>
    /// 百度接口签名帮助类
    /// </summary>
    public class BaiduApiHelper
    {
        private static readonly HttpClient HttpClient = new();

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="accessKeyId"> 百度AccessKeyId(AK) </param>
        /// <param name="secretAccessKey"> 百度SecretAccessKey(SK) </param>
        public BaiduApiHelper(string accessKeyId, string secretAccessKey)
        {
            _accessKeyId = accessKeyId;
            _secretAccessKey = secretAccessKey;
        }

        #endregion 构造函数

        #region 内部成员

        private string _accessKeyId { get; }
        private string _secretAccessKey { get; }

        private string CanonicalRequest(string method, Uri uri)
        {
            StringBuilder canonicalReq = new StringBuilder();
            canonicalReq.Append(method).Append("\n").Append(UriEncode(Uri.UnescapeDataString(uri.AbsolutePath))).Append("\n");

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(uri.Query);
            List<string> parameterStrings = new List<string>();
            foreach (KeyValuePair<string, string> entry in parameters)
            {
                parameterStrings.Add(UriEncode(entry.Key) + '=' + UriEncode(entry.Value));
            }
            parameterStrings.Sort();
            canonicalReq.Append(string.Join("&", parameterStrings.ToArray())).Append("\n");

            string host = uri.Host;
            if (!(uri.Scheme == "https" && uri.Port == 443) && !(uri.Scheme == "http" && uri.Port == 80))
            {
                host += ":" + uri.Port;
            }
            canonicalReq.Append("host:" + UriEncode(host));
            return canonicalReq.ToString();
        }

        private string Hex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private string UriEncode(string input, bool encodeSlash = false)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in Encoding.UTF8.GetBytes(input))
            {
                if ((b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9') || b == '_' || b == '-' || b == '~' || b == '.')
                {
                    builder.Append((char)b);
                }
                else if (b == '/')
                {
                    if (encodeSlash)
                    {
                        builder.Append("%2F");
                    }
                    else
                    {
                        builder.Append((char)b);
                    }
                }
                else
                {
                    builder.Append('%').Append(b.ToString("X2"));
                }
            }
            return builder.ToString();
        }

        #endregion 内部成员

        #region 外部接口

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="phoneNum"> 手机号码 </param>
        /// <param name="code"> 验证码 </param>
        /// <returns> </returns>
        public static bool SendMsg(string phoneNum, string code)
        {
            try
            {
                BaiduApiHelper baiduApiHelper = new BaiduApiHelper("", "");

                string host = "http://sms.bj.baidubce.com";
                string url = "/bce/v2/message";
                Dictionary<string, object> paramters = new Dictionary<string, object>
                {
                    { "invokeId", "SGf96VPF-WBBx-qbJK" },
                    { "phoneNumber", phoneNum },
                    { "templateCode", "smsTpl:e7476122a1c24e37b3b0de19d04ae900" },
                    { "contentVar", new { others = "你好", code = code } }
                };

                string resJsonStr = baiduApiHelper.RequestData("POST", host, url, paramters);
                JObject resJson = JsonConvert.DeserializeObject<JObject>(resJsonStr);

                return resJson["code"]?.ToString() == "1000";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="method"> 请求方法，需要大写，列如(POST) </param>
        /// <param name="host"> 主机地址列如(http://sms.bj.baidubce.com) </param>
        /// <param name="url"> 接口地址列如(/bce/v2/message) </param>
        /// <param name="paramters"> 参数列表 </param>
        /// <returns> </returns>
        public string RequestData(string method, string host, string url, Dictionary<string, object> paramters = null)
        {
            string ak = _accessKeyId;
            string sk = _secretAccessKey;
            DateTime now = DateTime.Now.ToCstTime();
            int expirationInSeconds = 1200;

            Uri uri = new Uri(host + url);
            string upperMethod = method?.ToUpperInvariant() ?? "POST";
            using var request = new HttpRequestMessage(new System.Net.Http.HttpMethod(upperMethod), uri);

            if (paramters != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(paramters), Encoding.UTF8, "application/json");
            }

            string signDate = now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");
            string authString = "bce-auth-v1/" + ak + "/" + signDate + "/" + expirationInSeconds;
            using var signingHmac = new HMACSHA256(Encoding.UTF8.GetBytes(sk));
            string signingKey = Hex(signingHmac.ComputeHash(Encoding.UTF8.GetBytes(authString)));

            string canonicalRequestString = CanonicalRequest(upperMethod, uri);

            using var signatureHmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));
            string signature = Hex(signatureHmac.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequestString)));
            string authorization = authString + "/host/" + signature;

            request.Headers.TryAddWithoutValidation("x-bce-date", signDate);
            request.Headers.TryAddWithoutValidation("Authorization", authorization);

            using HttpResponseMessage response = HttpClient.Send(request);
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        #endregion 外部接口
    }
}
