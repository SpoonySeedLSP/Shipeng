using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Shipeng.Util
{
    public static class WeChatMessageHelper
    {
        public static string GetAccessToken(string appId, string appsecret)
        {
            Dictionary<string, object> paramters = new Dictionary<string, object>{
                { "appid", appId },
                { "secret", appsecret },
                { "grant_type", "client_credential" }};

            JObject jo = JObject.Parse(HttpHelper.GetData("https://api.weixin.qq.com/cgi-bin/token", paramters));
            string access_token = jo["access_token"].ToString();
            return access_token;
        }

        /// <summary>
        /// 获取小程序码
        /// </summary>
        /// <param name="token"> token </param>
        /// <param name="scene"> 传入参数 </param>
        /// <param name="page"> 路径 </param>
        /// <param name="width"> 宽度 </param>
        /// <param name="path"> </param>
        /// <returns> </returns>
        public static string GetQRCode(string token, string scene, string page, int width = 280, string path = null)
        {
            string url = "https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + token + "";
            string postData = new
            {
                scene,
                page,
                width
            }.ToJson();
            HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(postData);
            request.ContentLength = payload.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream stream = response.GetResponseStream();
            List<byte> bytes = new List<byte>();
            int temp = stream.ReadByte();
            while (temp != -1)
            {
                bytes.Add((byte)temp);
                temp = stream.ReadByte();
            }
            byte[] result = bytes.ToArray();

            if (!path.IsNullOrEmpty())
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(result, 0, result.Length);
                    fs.Close();
                }
            }
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// 小程序发送订阅消息
        /// </summary>
        /// <param name="access_token"> </param>
        /// <param name="input"> </param>
        /// <returns> </returns>
        public static async Task SendWeChatMessagerAsync(string access_token, SubscribeMessageModel input)
        {
            //if (access_token.IsNullOrEmpty())
            //{
            //    throw new BusException(false, "接口调用凭证不存在", 1000);
            //}
            //if (input.Touser.IsNullOrEmpty())
            //{
            //    throw new BusException(false, "用户openid不存在", 1000);
            //}
            //if (input.TemplateId.IsNullOrEmpty())
            //{
            //    throw new BusException(false, "小程序模板ID不存在", 1000);
            //}
            //if (input.Page.IsNullOrEmpty())
            //{
            //    throw new BusException(false, "小程序页面路径不存在", 1000);
            //}

            //if (input.Data.IsNullOrEmpty())
            //{
            //    throw new BusException(false, "小程序模板数据不存在", 1000);
            //}
            input.MiniprogramState = "formal";
            input.Lang = "zh_TW";

            await Task.Run(() =>
             {
                 SendWeChatMessager(access_token, input);
             });
        }

        public static void SendWeChatMessager(string access_token, SubscribeMessageModel input)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/message/subscribe/send?access_token={0}", access_token);

            string result = HttpHelper.HttpPost(url, JsonConvert.SerializeObject(input), "application/json", null);
            ReturnReult data = result.ToObject<ReturnReult>();

            if (data.Errcode != 0)
            {
                LogHelper.WriteLog_LocalTxt("发送消息失败:" + data.Errmsg + ",data:" + input.ToJson());
            }
            else
            {
                //LogHelper.WriteLog_LocalTxt("发送消息成功:" + "data:" + input.ToJson());
            }
        }
    }

    /// <summary>
    /// 訂閲消息請求模型
    /// </summary>
    public class SubscribeMessageModel
    {
        /// <summary>
        　　　　/// 消息接收者的openid
        　　　　/// </summary>
        [JsonProperty("touser")]
        public string Touser { get; set; }

        /// <summary>
        　　　　/// 消息模板ID
        　　　　/// </summary>
        [JsonProperty("template_id")]
        public string TemplateId { get; set; }

        /// <summary>
        　　　　/// 點擊模板卡片后的跳轉頁面，僅限本小程序内的頁面，支持帶參數（示例index?foo=bar），該字段不填則不跳轉
        　　　　/// </summary>
        [JsonProperty("page")]
        public string Page { get; set; }

        /// <summary>
        　　　　/// 跳轉小程序類型：developer開發版、trial體驗版、formal正式版，默认为正式版
        　　　　/// </summary>
        [JsonProperty("miniprogram_state")]
        public string MiniprogramState { get; set; }

        /// <summary>
        　　　　/// 進入小程序查看的語言類型，支持zh_CN(簡體中文)、en_US(英文)、zh_HK(繁體中文)、zh_TW(繁體中文)，默認為zh_CN
        　　　　/// </summary>
        [JsonProperty("lang")]
        public string Lang { get; set; }

        /// <summary>
        /// 模板内容
        /// </summary>
        [JsonProperty("data")]
        public Dictionary<string, DataValue> Data { get; set; }
    }

    /// <summary>
    /// 模板内容關鍵字
    /// </summary>
    public class DataValue
    {
        /// <summary>
        /// 訂閲消息參數值
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class ReturnReult
    {
        public int Errcode { get; set; }

        public string Errmsg { get; set; }
    }
}