using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Shipeng.Util
{
    /// <summary>
    /// Http请求操作帮助类
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// 复用的 HTTP 客户端实例。
        /// </summary>
        /// <remarks>
        /// HttpClient 内部维护连接池，频繁创建和释放会导致端口耗尽、DNS 缓存行为不可控和额外
        /// TLS 握手成本。工具库中的通用请求方法应复用同一个实例，并在每次调用时创建独立的
        /// HttpRequestMessage 来承载请求头，避免不同调用之间的 Header 状态互相污染。
        /// </remarks>
        private static readonly HttpClient SharedHttpClient = new();

        /// <summary>
        /// 专用于抓取 HTML 的 HTTP 客户端实例。
        /// </summary>
        /// <remarks>
        /// HTML 抓取通常会遇到 gzip/deflate 压缩响应，因此这里单独配置自动解压。
        /// 仍然复用单例客户端，避免每次抓取都重新建立连接池。
        /// </remarks>
        private static readonly HttpClient HtmlHttpClient = new(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        #region 外部接口

        /// <summary>
        /// 记录日志
        /// </summary>
        public static Action<string> HandleLog { get; set; }

        /// <summary>
        /// 生成接口签名sign 注：md5(appId+time+guid+body+appSecret)
        /// </summary>
        /// <param name="appId"> 应用Id </param>
        /// <param name="appSecret"> 应用密钥 </param>
        /// <param name="guid"> 唯一GUID </param>
        /// <param name="time"> 时间 </param>
        /// <param name="body"> 请求体 </param>
        /// <returns> </returns>
        public static string BuildApiSign(string appId, string appSecret, string guid, DateTime time, string body)
        {
            return $"{appId}{time:yyyy-MM-dd HH:mm:ss}{guid}{body}{appSecret}".ToMD5String();
        }

        /// <summary>
        /// 构建完全Url
        /// </summary>
        /// <param name="url"> Url </param>
        /// <param name="parameters"> 参数 </param>
        /// <returns> </returns>
        public static string BuildGetFullUrl(string url, Dictionary<string, object> parameters = null)
        {
            StringBuilder paramBuilder = new StringBuilder();
            List<KeyValuePair<string, object>> paramList = new List<KeyValuePair<string, object>>();
            paramList = parameters?.ToList();
            for (int i = 0; i < paramList.Count; i++)
            {
                KeyValuePair<string, object> theParamter = paramList[i];
                string key = theParamter.Key;
                string value = theParamter.Value.ToString();

                string head;
                if (i == 0 && !UrlHaveParam(url))
                {
                    head = "?";
                }
                else
                {
                    head = "&";
                }

                paramBuilder.Append($@"{head}{key}={value}");
            }

            return url + paramBuilder.ToString();
        }

        /// <summary>
        /// 获取所有请求的参数（包括get参数和post参数）
        /// </summary>
        /// <param name="context"> 请求上下文 </param>
        /// <returns> </returns>
        public static async Task<Dictionary<string, object>> GetAllRequestParamsAsync(HttpContext context)
        {
            Dictionary<string, object> allParams = new Dictionary<string, object>();

            HttpRequest request = context.Request;

            List<string> paramKeys = new List<string>();
            List<string> getParams = request.Query.Keys.ToList();
            List<string> postParams = new List<string>();
            try
            {
                if (request.Method.ToLower() != "get")
                {
                    postParams = request.Form.Keys.ToList();
                }
            }
            catch
            {
            }
            paramKeys.AddRange(getParams);
            paramKeys.AddRange(postParams);

            paramKeys.ForEach(aParam =>
            {
                object value = null;
                if (request.Query.ContainsKey(aParam))
                {
                    value = request.Query[aParam].ToString();
                }
                else if (request.Form.ContainsKey(aParam))
                {
                    value = request.Form[aParam].ToString();
                }

                allParams.Add(aParam, value);
            });

            string contentType = request.ContentType?.ToLower() ?? "";

            //若为POST的application/json
            if (contentType.Contains("application/json"))
            {
                if (request.Body != null)
                {
                    string body = await request.Body.ReadToStringAsync();
                    Newtonsoft.Json.Linq.JObject obj = body.ToJObject();
                    foreach (KeyValuePair<string, Newtonsoft.Json.Linq.JToken> aProperty in obj)
                    {
                        allParams[aProperty.Key] = aProperty.Value;
                    }
                }
            }
            return allParams;
        }

        /// <summary>
        /// 发起GET请求 注：若使用证书,推荐使用X509Certificate2的pkcs12证书
        /// </summary>
        /// <param name="url"> 地址 </param>
        /// <param name="paramters"> 参数 </param>
        /// <param name="headers"> 请求头 </param>
        /// <param name="cerFile"> 证书 </param>
        /// <returns> </returns>
        public static string GetData(string url, Dictionary<string, object> paramters = null, Dictionary<string, string> headers = null, X509Certificate cerFile = null)
        {
            return RequestData(HttpMethod.Get, url, paramters, headers, ContentType.Form, cerFile);
        }

        /// <summary>
        /// 从URL获取html文档
        /// </summary>
        /// <param name="url"> </param>
        /// <returns> </returns>
        public static string GetHtml(string url)
        {
            string htmlCode;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                    request.Headers.UserAgent.ParseAdd("Mozilla/4.0");
                    using HttpResponseMessage response = HtmlHttpClient.Send(request);
                    response.EnsureSuccessStatusCode();
                    htmlCode = ReadHttpContentAsString(response.Content);
                    return htmlCode;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("重试中....................");
                }
            }

            return "";
        }

        /// <summary>
        /// 发起POST请求 注：若使用证书,推荐使用X509Certificate2的pkcs12证书
        /// </summary>
        /// <param name="url"> 地址 </param>
        /// <param name="paramters"> 参数 </param>
        /// <param name="headers"> 请求头 </param>
        /// <param name="contentType"> 请求的ContentType </param>
        /// <param name="cerFile"> 证书 </param>
        /// <returns> </returns>
        public static string PostData(string url, Dictionary<string, object> paramters = null, Dictionary<string, string> headers = null, ContentType contentType = ContentType.Form, X509Certificate cerFile = null)
        {
            Dictionary<ContentType, string> mapping = new Dictionary<ContentType, string>
            {
                { ContentType.Form, "application/x-www-form-urlencoded" },
                { ContentType.Json, "application/json" }
            };

            string body = BuildBody(paramters, contentType);
            return PostData(url, body, mapping[contentType], headers, cerFile);
        }

        /// <summary>
        /// 发起POST请求 注：若使用证书,推荐使用X509Certificate2的pkcs12证书
        /// </summary>
        /// <param name="url"> 地址 </param>
        /// <param name="body"> 请求体 </param>
        /// <param name="contentType"> 请求的ContentType </param>
        /// <param name="headers"> 请求头 </param>
        /// <param name="cerFile"> 证书 </param>
        /// <returns> </returns>
        public static string PostData(string url, string body, string contentType, Dictionary<string, string> headers, X509Certificate cerFile)
        {
            return RequestData("POST", url, body, contentType, headers, cerFile);
        }

        /// <summary>
        /// 请求数据 注：若使用证书,推荐使用X509Certificate2的pkcs12证书
        /// </summary>
        /// <param name="method"> 请求方法 </param>
        /// <param name="url"> URL地址 </param>
        /// <param name="paramters"> 参数 </param>
        /// <param name="headers"> 请求头信息 </param>
        /// <param name="contentType"> 请求数据类型 </param>
        /// <param name="cerFile"> 证书 </param>
        /// <returns> </returns>
        public static string RequestData(HttpMethod method, string url, Dictionary<string, object> paramters = null, Dictionary<string, string> headers = null, ContentType contentType = ContentType.Form, X509Certificate cerFile = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("请求地址不能为NULL或空！");
            }

            string newUrl = url;
            if (method == HttpMethod.Get)
            {
                StringBuilder paramBuilder = new StringBuilder();
                List<KeyValuePair<string, object>> paramList = new List<KeyValuePair<string, object>>();
                paramList = paramters?.ToList() ?? new List<KeyValuePair<string, object>>();
                for (int i = 0; i < paramList.Count; i++)
                {
                    KeyValuePair<string, object> theParamter = paramList[i];
                    string key = theParamter.Key;
                    string value = theParamter.Value.ToString();

                    string head;
                    if (i == 0 && !UrlHaveParam(url))
                    {
                        head = "?";
                    }
                    else
                    {
                        head = "&";
                    }

                    paramBuilder.Append($@"{head}{key}={value}");
                }

                newUrl = url + paramBuilder.ToString();
            }

            string body = BuildBody(paramters, contentType);
            return RequestData(method.ToString().ToUpper(), newUrl, body, GetContentTypeStr(contentType), headers, cerFile);
        }

        /// <summary>
        /// 请求数据 注：若使用证书,推荐使用X509Certificate2的pkcs12证书
        /// </summary>
        /// <param name="method"> 请求方法 </param>
        /// <param name="url"> 请求地址 </param>
        /// <param name="body"> 请求的body内容 </param>
        /// <param name="contentType"> 请求数据类型 </param>
        /// <param name="headers"> 请求头 </param>
        /// <param name="cerFile"> 证书 </param>
        /// <returns> </returns>
        public static string RequestData(string method, string url, string body, string contentType, Dictionary<string, string> headers = null, X509Certificate cerFile = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("请求地址不能为NULL或空！");
            }

            string resData = string.Empty;
            DateTime startTime = DateTime.Now;
            try
            {
                using var request = new HttpRequestMessage(new System.Net.Http.HttpMethod(method.ToUpper()), url);
                headers?.ForEach(aHeader =>
                {
                    request.Headers.TryAddWithoutValidation(aHeader.Key, aHeader.Value);
                });

                if (method.ToUpper() != "GET")
                {
                    request.Content = new StringContent(body ?? string.Empty, Encoding.UTF8);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                }

                HttpClient client = SharedHttpClient;
                using HttpClientHandler handler = cerFile == null ? null : new HttpClientHandler();
                using HttpClient certificateClient = cerFile == null ? null : new HttpClient(handler, disposeHandler: false);
                if (cerFile != null)
                {
                    handler.ClientCertificates.Add(cerFile);
                    client = certificateClient;
                }

                using HttpResponseMessage response = client.Send(request);
                response.EnsureSuccessStatusCode();
                resData = ReadHttpContentAsString(response.Content);

                return resData;
            }
            catch (Exception ex)
            {
                resData = $"异常:{ExceptionHelper.GetExceptionAllMsg(ex)}";

                throw;
            }
            finally
            {
                TimeSpan time = DateTime.Now - startTime;
                if (resData?.Length > 1000)
                {
                    resData = new string(resData.Copy(0, 1000).ToArray());
                    resData += "......";
                }

                string log =
$@"方向:请求外部接口
url:{url}
method:{method}
contentType:{contentType}
body:{body}
耗时:{(int)time.TotalMilliseconds}ms

返回:{resData}
";
                HandleLog?.Invoke(log);
            }
        }

        /// <summary>
        /// 发起安全签名请求 注：使用本框架签名算法,ContentType为application/json
        /// </summary>
        /// <param name="url"> 地址 </param>
        /// <param name="body"> 请求body </param>
        /// <param name="appId"> 应用Id </param>
        /// <param name="appSecret"> 应用密钥 </param>
        /// <returns> </returns>
        public static string SafeSignRequest(string url, string body, string appId, string appSecret)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string guid = Guid.NewGuid().ToString();
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {"appId",appId },
                {"time",time },
                {"guid",guid },
                {"sign",BuildApiSign(appId,appSecret,guid,time.ToDateTime(),body) }
            };

            return RequestData("post", url, body, "application/json", headers);
        }

        /// <summary>
        /// 模拟POST提交
        /// </summary>
        /// <param name="url"> 请求地址 </param>
        /// <param name="xmlParam"> xml参数 </param>
        /// <returns> 返回结果 </returns>
        public static string PostHttpResponse(string url, string xmlParam)
        {
            try
            {
                using var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url)
                {
                    Content = new StringContent(xmlParam ?? string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                request.Content.Headers.ContentType.CharSet = "utf-8";

                using HttpResponseMessage result = SharedHttpClient.Send(request);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return ReadHttpContentAsString(result.Content);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new BusException(false, ex.Message, 1000);
            }
        }

        /// <summary>
        　　　　/// post同步請求
        　　　　/// </summary>
        　　　　/// <param name="url">地址</param>
        　　　　/// <param name="postData">數據</param>
        　　　　/// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        　　　　/// <param name="headers">請求頭</param>
        　　　　/// <returns></returns>
        public static string HttpPost(string url, string postData = null, string contentType = null, Dictionary<string, string> headers = null)
        {
            return HttpPostAsync(url, postData, contentType, 30, headers).GetAwaiter().GetResult();
        }

        /// <summary>
        　　　　/// post異步請求
        　　　　/// </summary>
        　　　　/// <param name="url">地址</param>
        　　　　/// <param name="postData">數據</param>
        　　　　/// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        　　　　/// <param name="timeOut">請求超時時間</param>
        　　　　/// <param name="headers">請求頭</param>
        　　　　/// <returns></returns>
        public static async Task<string> HttpPostAsync(string url, string postData = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("请求地址不能为空。", nameof(url));
            }

            using var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            postData ??= "";
            request.Content = new StringContent(postData, Encoding.UTF8);
            if (contentType != null)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(timeOut, 1)));
            using HttpResponseMessage response = await SharedHttpClient.SendAsync(request, cancellationTokenSource.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        }

        #endregion 外部接口

        #region 内部成员

        private static string BuildBody(Dictionary<string, object> parameters, ContentType contentType)
        {
            StringBuilder bodyBuilder = new StringBuilder();
            switch (contentType)
            {
                case ContentType.Form:
                    {
                        List<KeyValuePair<string, object>> paramList = parameters?.ToList() ?? new List<KeyValuePair<string, object>>();
                        for (int i = 0; i < paramList.Count; i++)
                        {
                            KeyValuePair<string, object> theParamter = paramList[i];
                            string key = theParamter.Key;
                            string value = theParamter.Value?.ToString();

                            string head = string.Empty;
                            if (i != 0)
                            {
                                head = "&";
                            }

                            bodyBuilder.Append($@"{head}{key}={value}");
                        }
                    }; break;
                case ContentType.Json:
                    {
                        bodyBuilder.Append(JsonConvert.SerializeObject(parameters));
                    }; break;
                default: break;
            }

            return bodyBuilder.ToString();
        }

        private static string GetContentTypeStr(ContentType contentType)
        {
            string contentTypeStr = string.Empty;
            switch (contentType)
            {
                case ContentType.Form: contentTypeStr = "application/x-www-form-urlencoded"; break;
                case ContentType.Json: contentTypeStr = "application/json"; break;
                default: break;
            }

            return contentTypeStr;
        }

        private static bool UrlHaveParam(string url)
        {
            return url.Contains("?");
        }

        /// <summary>
        /// 按响应头中的字符集读取 HTTP 响应文本；没有声明字符集时使用 UTF-8。
        /// </summary>
        /// <remarks>
        /// 统一从这里读取响应可以避免各个请求方法散落不同编码策略，也便于后续扩展
        /// BOM 检测、HTML meta charset 检测等更复杂的兼容逻辑。
        /// </remarks>
        private static string ReadHttpContentAsString(HttpContent content)
        {
            Encoding encoding = Encoding.UTF8;
            string charset = content.Headers.ContentType?.CharSet;
            if (!string.IsNullOrWhiteSpace(charset))
            {
                encoding = Encoding.GetEncoding(charset.Trim('"'));
            }

            using Stream stream = content.ReadAsStream();
            using StreamReader reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }

        #endregion 内部成员
    }

    #region 类型定义

    public enum ContentType
    {
        /// <summary>
        /// 传统Form表单,即application/x-www-form-urlencoded
        /// </summary>
        Form,

        /// <summary>
        /// 使用Json,即application/json
        /// </summary>
        Json
    }

    /// <summary>
    /// Http请求方法定义
    /// </summary>
    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete,
        Head,
        Options,
        Trace,
        Connect
    }

    #endregion 类型定义
}
