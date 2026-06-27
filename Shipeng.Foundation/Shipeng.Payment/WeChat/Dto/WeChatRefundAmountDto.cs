using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信退款金额信息。
    /// </summary>
    [SuppressSniffer]
    public class WeChatRefundAmountDto
    {
        /// <summary>
        /// 退款金额，单位：分。
        /// </summary>
        public int refund { get; set; }

        /// <summary>
        /// 用户支付金额，单位：分。
        /// </summary>
        public int payer_total { get; set; }

        /// <summary>
        /// 用户退款金额，单位：分。
        /// </summary>
        public int payer_refund { get; set; }

        /// <summary>
        /// 原订单金额，单位：分。
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 币种，一般为 CNY。
        /// </summary>
        public string currency { get; set; }
    }
}
