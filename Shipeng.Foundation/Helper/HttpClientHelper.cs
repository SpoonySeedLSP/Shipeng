using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;
using System.Text;

namespace Shipeng.Util
{
    /// <summary>
    /// Http 请求帮助类。
    /// </summary>
    public static class HttpClientHelper
    {
        private static readonly HttpClient Client = new HttpClient(new HttpClientHandler { UseCookies = false });

        /// <summary>
        /// 使用 POST 发送字典数据，字典会先序列化为 JSON。
        /// </summary>
        /// <param name="url">目标地址。</param>
        /// <param name="data">请求数据。</param>
        /// <returns>响应字符串。</returns>
        public static Task<string> PostAsync(string url, Dictionary<string, string> data)
        {
            return PostJsonAsync(url, data.ToJson());
        }

        /// <summary>
        /// 使用 POST 发送字典数据，字典会先序列化为 JSON。
        /// </summary>
        /// <param name="url">目标地址。</param>
        /// <param name="data">请求数据。</param>
        /// <param name="header">请求头。</param>
        /// <returns>响应字符串。</returns>
        public static Task<string> PostAsync(string url, Dictionary<string, string> data, Dictionary<string, string> header = null)
        {
            return PostAsync(url, data.ToJson(), header);
        }

        /// <summary>
        /// 使用 POST 发送 JSON 字符串。
        /// </summary>
        /// <param name="url">目标地址。</param>
        /// <param name="json">JSON 请求体。</param>
        /// <returns>响应字符串。</returns>
        public static Task<string> PostJsonAsync(string url, string json)
        {
            return PostAsync(url, json);
        }

        /// <summary>
        /// 使用 POST 发送 JSON 字符串。
        /// </summary>
        /// <param name="url">目标地址。</param>
        /// <param name="data">JSON 请求体。</param>
        /// <param name="header">请求头。</param>
        /// <param name="Gzip">响应内容是否按 GZip 解压。</param>
        /// <returns>响应字符串。</returns>
        public static async Task<string> PostAsync(string url, string data, Dictionary<string, string> header = null, bool Gzip = false)
        {
            using var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url)
            {
                Content = new StringContent(data ?? string.Empty, Encoding.UTF8, "application/json")
            };
            AddHeaders(request, header);

            using HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await ReadResponseAsync(response, Gzip).ConfigureAwait(false);
        }

        /// <summary>
        /// 使用 GET 请求字符串内容。
        /// </summary>
        /// <param name="url">目标地址。</param>
        /// <param name="header">请求头。</param>
        /// <param name="Gzip">响应内容是否按 GZip 解压。</param>
        /// <returns>响应字符串。</returns>
        public static async Task<string> GetAsync(string url, Dictionary<string, string> header = null, bool Gzip = false)
        {
            using var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
            AddHeaders(request, header);

            using HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await ReadResponseAsync(response, Gzip).ConfigureAwait(false);
        }

        /// <summary>
        /// 使用 GET 请求字符串内容。
        /// </summary>
        /// <param name="url">目标地址。</param>
        /// <param name="data">查询参数。</param>
        /// <param name="header">请求头。</param>
        /// <returns>响应字符串。</returns>
        public static Task<string> GetAsync(string url, Dictionary<string, object> data, Dictionary<string, string> header = null)
        {
            if (data == null || data.Count == 0)
            {
                return GetAsync(url, header);
            }

            StringBuilder paramBuilder = new StringBuilder();
            string separator = UrlHaveParam(url) ? "&" : "?";
            foreach (KeyValuePair<string, object> item in data)
            {
                string key = Uri.EscapeDataString(item.Key ?? string.Empty);
                string value = Uri.EscapeDataString(item.Value?.ToString() ?? string.Empty);
                paramBuilder.Append(separator).Append(key).Append('=').Append(value);
                separator = "&";
            }

            return GetAsync(url + paramBuilder, header);
        }

        private static bool UrlHaveParam(string url)
        {
            return url.Contains("?");
        }

        /// <summary>
        /// 使用 POST 请求并反序列化响应对象。
        /// </summary>
        /// <typeparam name="T">响应对象类型。</typeparam>
        /// <typeparam name="T2">请求对象类型。</typeparam>
        /// <param name="url">目标地址。</param>
        /// <param name="obj">请求对象。</param>
        /// <returns>响应对象。</returns>
        public static async Task<T> PostObjectAsync<T, T2>(string url, T2 obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            string responseBody = await PostJsonAsync(url, json).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(responseBody);
        }

        /// <summary>
        /// 使用 GET 请求并反序列化响应对象。
        /// </summary>
        /// <typeparam name="T">响应对象类型。</typeparam>
        /// <param name="url">目标地址。</param>
        /// <returns>响应对象。</returns>
        public static async Task<T> GetObjectAsync<T>(string url)
        {
            string responseBody = await GetAsync(url).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(responseBody);
        }

        private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers == null) return;

            foreach (KeyValuePair<string, string> item in headers)
            {
                request.Headers.TryAddWithoutValidation(item.Key, item.Value);
            }
        }

        private static async Task<string> ReadResponseAsync(HttpResponseMessage response, bool gzip)
        {
            if (!gzip)
            {
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            await using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using GZipInputStream gzipStream = new GZipInputStream(responseStream);
            using StreamReader reader = new StreamReader(gzipStream, Encoding.UTF8);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
