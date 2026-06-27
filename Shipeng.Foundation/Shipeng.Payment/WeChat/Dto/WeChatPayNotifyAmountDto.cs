using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信支付回调金额信息。
    /// </summary>
    [SuppressSniffer]
    public class WeChatPayNotifyAmountDto
    {
        /// <summary>
        /// 订单总金额，单位：分。
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 用户实际支付金额，单位：分。
        /// </summary>
        public int payer_total { get; set; }

        /// <summary>
        /// 货币类型。
        /// 一般为 CNY。
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// 用户支付币种。
        /// 一般为 CNY。
        /// </summary>
        public string payer_currency { get; set; }
    }
}
