using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Shipeng.Util.API
{
    /// <summary>
    /// 华为短信接口
    /// </summary>
    public class HuaWeiSendSms
    {
        #region 公共变量

        private static readonly string apiAddress = "https://rtcsms.cn-north-1.myhuaweicloud.com:10743/sms/batchSendSms/v1"; //APP接入地址+接口访问URI
        private static readonly string appKey = "R4i5yYZ246I6wDy871GAnEU2NnfS"; //APP_Key
        private static readonly string appSecret = "pchoXRf1l64PPgk7W0iJHo908R70"; //APP_Secret
        private static readonly HttpClient HttpClient = new HttpClient();

        private static readonly List<SmsDTO> _smsDTOs = new List<SmsDTO> {
            new SmsDTO { Id = SmsTypes.Test, SemplateId = "2865aaca41c24e76a713749f08993a1b", SenderId = "99200620888880002766", Signature = "华为云短信测试" },//测试
            new SmsDTO { Id = SmsTypes.VeriCode, SemplateId = "6c55905b977e4516a571766144115517", SenderId = "8821051236522", Signature = "远致科技" },//正式
            new SmsDTO { Id = SmsTypes.VeriCodeA, SemplateId = "38de39afa6124d96832448d4a9972c5b", SenderId = "8821041424408", Signature = "远致地产" },//正式
            new SmsDTO { Id = SmsTypes.ConsultantReception, SemplateId = "5b2215732ef849e695fe67a03168e045", SenderId = "8821052011958", Signature = "远致科技" },//顾问接待通知
            new SmsDTO { Id = SmsTypes.CustomerDistributionManager, SemplateId = "6f43ca53959340029417687f271bf097", SenderId = "8821052011958", Signature = "远致科技" },//顾问接待通知
        };

        #endregion 公共变量

        /// <summary>
        /// 短信模板
        /// </summary>
        public enum SmsTypes
        {
            [Description("测试")]
            Test = 0,

            [Description("验证码")]
            VeriCode = 1,

            [Description("验证码,远致地产")]
            VeriCodeA = 2,

            /// <summary>
            /// 您有${1}位客户需要接待,请打开APP及时接待！
            /// </summary>
            [Description("顾问接待通知")]
            ConsultantReception = 3,

            /// <summary>
            /// 您有${1}位客户需要分配,请打开APP及时接待！
            /// </summary>
            [Description("客户分配经理收")]
            CustomerDistributionManager = 4,
        }

        /// <summary>
        /// 实体
        /// </summary>
        public class SmsDTO
        {
            /// <summary>
            /// id
            /// </summary>
            public SmsTypes Id { get; set; }

            /// <summary>
            /// 模板id
            /// </summary>
            public string SemplateId { get; set; }

            /// <summary>
            /// 签名id
            /// </summary>
            public string SenderId { get; set; }

            /// <summary>
            /// 签名
            /// </summary>
            public string Signature { get; set; }
        }

        #region 发送短信

        /// <summary>
        /// 是否是手机号码
        /// </summary>
        /// <param name="val"></param>
        public static bool IsMobile(string val)
        {
            return Regex.IsMatch(val, @"^1\d{10}$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="Mobile"> 手机号 </param>
        /// <param name="Paras"> 变量 </param>
        /// <param name="Id"> 模板id </param>
        public static async Task<bool> SMSAsync(List<string> Mobile, List<string> Paras, SmsTypes Id)
        {
            SmsDTO sms = _smsDTOs.Where(x => x.Id == Id).FirstOrDefault();

            if (!sms.IsNullOrEmpty())
            {
                //必填,全局号码格式(包含国家码),示例:+8615123456789,多个号码之间用英文逗号分隔
                string receiver = ""; //+8615123456789,+8615234567890//短信接收人号码
                foreach (string i in Mobile)
                {
                    if (IsMobile(i))
                    {
                        receiver += "+86" + i + ",";
                    }
                }
                if (!receiver.IsNullOrEmpty())
                {
                    receiver = receiver.TrimEnd(',');
                }
                else
                {
                    throw new BusException("手机号错误");
                }
                //选填,短信状态报告接收地址,推荐使用域名,为空或者不填表示不接收状态报告
                string statusCallBack = "";

                /*
                 * 选填,使用无变量模板时请赋空值 string templateParas = "";
                 * 单变量模板示例:模板内容为"您的验证码是${1}"时,templateParas可填写为"[\"369751\"]"
                 * 双变量模板示例:模板内容为"您有${1}件快递请到${2}领取"时,templateParas可填写为"[\"3\",\"人民公园正门\"]"
                 * 模板中的每个变量都必须赋值，且取值不能为空
                 * 查看更多模板和变量规范:产品介绍>模板和变量规范
                 */
                string templateParas = JsonConvert.SerializeObject(Paras);
                //"[\"369751\"]"; //模板变量，此处以单变量验证码短信为例，请客户自行生成6位验证码，并定义为字符串类型，以杜绝首位0丢失的问题（例如：002569变成了2569）。

                //请求Body
                Dictionary<string, string> body = new Dictionary<string, string>() {
                    {"from", sms.SenderId},
                    {"to", receiver},
                    {"templateId", sms.SemplateId},
                    {"templateParas", templateParas},
                    {"statusCallback", statusCallBack},
                    {"signature",sms.Signature} //使用国内短信通用模板时,必须填写签名名称
                };

                using HttpContent content = new FormUrlEncodedContent(body);
                using HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, apiAddress)
                {
                    Content = content
                };
                //请求Headers。每次请求单独设置，避免复用 HttpClient 时多个请求之间的默认请求头互相污染。
                request.Headers.TryAddWithoutValidation("Authorization", "WSSE realm=\"SDP\",profile=\"UsernameToken\",type=\"Appkey\"");
                request.Headers.TryAddWithoutValidation("X-WSSE", BuildWSSEHeader(appKey, appSecret));
                //https://support.huaweicloud.com/devg-msgsms/sms_04_0009.html 错误代码
                using HttpResponseMessage response = await HttpClient.SendAsync(request);
                string res = await response.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(res);
                string pacage = jsonObj["code"].ToString();// {"code":"E000105","description":"Invalid digest."}
                if (pacage != "000000")
                {
                    pacage = pacage + "--" + jsonObj["description"].ToString();
                    //throw new BusException("短信发送失败错误:" + pacage + "");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                // throw new BusException("短信签名模板错误");
                return false;
            }
        }

        /// <summary>
        /// 构造X-WSSE参数值
        /// </summary>
        /// <param name="appKey"> </param>
        /// <param name="appSecret"> </param>
        /// <returns> </returns>
        private static string BuildWSSEHeader(string appKey, string appSecret)
        {
            string now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"); //Created
            string nonce = Guid.NewGuid().ToString().Replace("-", ""); //Nonce

            byte[] material = Encoding.UTF8.GetBytes(nonce + now + appSecret);
            using SHA256 sha256 = SHA256.Create();
            byte[] hashed = sha256.ComputeHash(material);
            string hexdigest = BitConverter.ToString(hashed).Replace("-", "");
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(hexdigest)); //PasswordDigest

            return string.Format("UsernameToken Username=\"{0}\",PasswordDigest=\"{1}\",Nonce=\"{2}\",Created=\"{3}\"", appKey, base64, nonce, now);
        }

        #endregion 发送短信
    }
}
