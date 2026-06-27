using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信退款回调解密后的结果。
    /// </summary>
    [SuppressSniffer]
    public class WeChatRefundNotifyResourceDto
    {
        /// <summary>微信支付商户号。</summary>
        public string mchid { get; set; }

        /// <summary>商户订单号。</summary>
        public string out_trade_no { get; set; }

        /// <summary>微信支付订单号。</summary>
        public string transaction_id { get; set; }

        /// <summary>商户退款单号。</summary>
        public string out_refund_no { get; set; }

        /// <summary>微信退款单号。</summary>
        public string refund_id { get; set; }

        /// <summary>
        /// 退款状态。
        /// SUCCESS：退款成功。
        /// CLOSED：退款关闭。
        /// ABNORMAL：退款异常。
        /// </summary>
        public string refund_status { get; set; }

        /// <summary>
        /// 退款成功时间。
        /// </summary>
        public DateTime? success_time { get; set; }

        /// <summary>
        /// 退款金额信息。
        /// </summary>
        public WeChatRefundAmountDto amount { get; set; }
    }

}
