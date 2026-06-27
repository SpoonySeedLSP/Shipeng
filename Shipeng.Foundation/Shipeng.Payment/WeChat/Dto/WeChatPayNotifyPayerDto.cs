using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信支付回调付款用户信息。
    /// </summary>
    [SuppressSniffer]
    public class WeChatPayNotifyPayerDto
    {
        /// <summary>
        /// 用户在当前 AppId 下的 openid。
        /// </summary>
        public string openid { get; set; }
    }
}
