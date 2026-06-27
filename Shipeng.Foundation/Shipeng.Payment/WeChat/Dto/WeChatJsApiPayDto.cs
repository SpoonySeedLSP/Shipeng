using System.Text.Json.Serialization;
using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// JSAPI / 小程序调起微信支付参数。
    /// </summary>
    [SuppressSniffer]
    public class WeChatJsApiPayDto
    {
        /// <summary>
        /// 小程序或公众号 AppId。
        /// </summary>
        public string appId { get; set; }

        /// <summary>
        /// 时间戳，单位秒。
        /// 微信前端支付参数要求字段名为 timeStamp。
        /// </summary>
        public string timeStamp { get; set; }

        /// <summary>
        /// 随机字符串。
        /// </summary>
        public string nonceStr { get; set; }

        /// <summary>
        /// 订单详情扩展字符串。
        /// 格式固定为：prepay_id=xxx。
        /// 注意字段名是 package，C# 关键字冲突，所以属性名仍用小写 package，并加 JsonPropertyName。
        /// </summary>
        [JsonPropertyName("package")]
        public string package { get; set; }

        /// <summary>
        /// 签名方式。
        /// 微信支付 V3 使用 RSA。
        /// </summary>
        public string signType { get; set; } = "RSA";

        /// <summary>
        /// 前端调起支付签名。
        /// 使用商户 API 私钥对 appId、timeStamp、nonceStr、package 生成签名。
        /// </summary>
        public string paySign { get; set; }
    }
}
