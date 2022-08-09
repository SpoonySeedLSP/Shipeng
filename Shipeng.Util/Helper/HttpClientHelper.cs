using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Shipeng.Util
{
    /// <summary>
    /// Http请求帮助类(新)
    /// </summary>
    public static class HttpClientHelper
    {
        private static readonly ServiceProvider _serviceProvider =
            new ServiceCollection().AddHttpClient().BuildServiceProvider();

        private static async Task HttpClientFactoryTest()
        {
            IHttpClientFactory httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            HttpClient client = httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://www.aliyun11.com/"));
            string content = await response.Content.ReadAsStringAsync();

            string tmp = string.Empty;
        }

        /// <summary>
        /// </summary>
        /// <param name="url"> url 传入参数 </param>
        /// <param name="data"> </param>
        /// <returns> </returns>
        public static async Task<string> PostAsync(string url, Dictionary<string, string> data)
        {
            return await PostJsonAsync(url, data.ToJson());
        }

        /// <summary>
        /// </summary>
        /// <param name="url"> url 传入参数 </param>
        /// <param name="data"> </param>
        /// <param name="header"> </param>
        /// <returns> </returns>
        public static async Task<string> PostAsync(string url, Dictionary<string, string> data, Dictionary<string, string> header = null)
        {
            return await PostAsync(url, data.ToJson(), header);
        }

        /// <summary>
        /// 使用post方法异步请求
        /// </summary>
        /// <param name="url"> 目标链接 </param>
        /// <param name="json"> 发送的参数字符串，只能用json </param>
        /// <returns> 返回的字符串 </returns>
        public static async Task<string> PostJsonAsync(string url, string json)
        {
            HttpClient client = new HttpClient();
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        /// <summary>
        /// 使用post方法异步请求
        /// </summary>
        /// <param name="url"> 目标链接 </param>
        /// <param name="data"> 发送的参数字符串 </param>
        /// <param name="header"> </param>
        /// <param name="Gzip"> </param>
        /// <returns> 返回的字符串 </returns>
        public static async Task<string> PostAsync(string url, string data, Dictionary<string, string> header = null, bool Gzip = false)
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { UseCookies = false });
            HttpContent content = new StringContent(data);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (KeyValuePair<string, string> item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseBody = "";
            if (Gzip)
            {
                GZipInputStream inputStream = new GZipInputStream(await response.Content.ReadAsStreamAsync());
                responseBody = new StreamReader(inputStream).ReadToEnd();
            }
            else
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }
            return responseBody;
        }

        /// <summary>
        /// 使用get方法异步请求
        /// </summary>
        /// <param name="url"> 目标链接 </param>
        /// <param name="header"> </param>
        /// <param name="Gzip"> </param>
        /// <returns> 返回的字符串 </returns>
        public static async Task<string> GetAsync(string url, Dictionary<string, string> header = null, bool Gzip = false)
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { UseCookies = false });
            if (header != null)
            {
                client.DefaultRequestHeaders.Clear();
                foreach (KeyValuePair<string, string> item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();//用来抛异常的
            string responseBody = "";
            if (Gzip)
            {
                GZipInputStream inputStream = new GZipInputStream(await response.Content.ReadAsStreamAsync());
                responseBody = new StreamReader(inputStream).ReadToEnd();
            }
            else
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }
            return responseBody;
        }

        /// <summary>
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="data"> </param>
        /// <param name="header"> </param>
        /// <returns> </returns>
        public static async Task<string> GetAsync(string url, Dictionary<string, object> data, Dictionary<string, string> header = null)
        {
            StringBuilder paramBuilder = new StringBuilder();
            int i = 0;

            string head;
            string key = "";
            string value = "";
            foreach (KeyValuePair<string, object> k in data)
            {
                key = k.Key;
                value = k.Value.ToString();
                if (i == 0 && !UrlHaveParam(url))
                {
                    head = "?";
                }
                else
                {
                    head = "&";
                }
                i++;

                paramBuilder.Append($@"{head}{key}={value}");
            }

            url = url + paramBuilder;
            return await GetAsync(url, header);
        }

        private static bool UrlHaveParam(string url)
        {
            return url.Contains("?");
        }

        /// <summary>
        /// 使用post返回异步请求直接返回对象
        /// </summary>
        /// <typeparam name="T"> 返回对象类型 </typeparam>
        /// <typeparam name="T2"> 请求对象类型 </typeparam>
        /// <param name="url"> 请求链接 </param>
        /// <param name="obj"> 请求对象数据 </param>
        /// <returns> 请求返回的目标对象 </returns>
        public static async Task<T> PostObjectAsync<T, T2>(string url, T2 obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            string responseBody = await PostJsonAsync(url, json); //请求当前账户的信息
            return JsonConvert.DeserializeObject<T>(responseBody);//把收到的字符串序列化
        }

        /// <summary>
        /// 使用Get返回异步请求直接返回对象
        /// </summary>
        /// <typeparam name="T"> 请求对象类型 </typeparam>
        /// <param name="url"> 请求链接 </param>
        /// <returns> 返回请求的对象 </returns>
        public static async Task<T> GetObjectAsync<T>(string url)
        {
            string responseBody = await GetAsync(url); //请求当前账户的信息
            return JsonConvert.DeserializeObject<T>(responseBody);//把收到的字符串序列化
        }
    }
}