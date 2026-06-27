using System.ComponentModel;
using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信支付交易类型。
    /// </summary>
    [SuppressSniffer]
    public enum WeChatPayTradeTypeEnum
    {
        /// <summary>
        /// JSAPI 支付。
        /// 适用于微信公众号网页支付、小程序支付。
        /// 需要传 openId。
        /// </summary>
        [Description("JSAPI 支付")]
        JsApi = 0,

        /// <summary>
        /// Native 支付。
        /// 适用于 PC 网页扫码支付。
        /// 返回 code_url，前端生成二维码给用户扫码。
        /// </summary>
        [Description("Native 支付")]
        Native = 1
    }
}
