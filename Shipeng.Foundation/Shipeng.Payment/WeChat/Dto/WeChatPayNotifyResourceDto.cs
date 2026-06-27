using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信支付回调解密后的支付结果。
    ///
    /// 说明：
    /// 1. 这是 resource.ciphertext 解密后的明文模型。
    /// 2. 业务模块应该根据 out_trade_no 查询本地支付单。
    /// 3. 业务模块必须校验金额，不能只看支付成功状态。
    /// 4. 微信回调可能重复推送，业务模块必须做幂等处理。
    /// </summary>
    [SuppressSniffer]
    public class WeChatPayNotifyResourceDto
    {
        /// <summary>
        /// 小程序、公众号或 App 的 AppId。
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 微信支付商户号。
        /// </summary>
        public string mchid { get; set; }

        /// <summary>
        /// 商户订单号。
        /// 这是系统创建微信支付订单时传给微信的 out_trade_no。
        /// 业务模块通过它查询本地支付单。
        /// </summary>
        public string out_trade_no { get; set; }

        /// <summary>
        /// 微信支付订单号。
        /// 支付成功后由微信生成。
        /// 后续退款、对账、查询会用到。
        /// </summary>
        public string transaction_id { get; set; }

        /// <summary>
        /// 交易类型。
        /// 常见值：
        /// JSAPI：公众号/小程序支付。
        /// NATIVE：扫码支付。
        /// APP：App支付。
        /// MICROPAY：付款码支付。
        /// </summary>
        public string trade_type { get; set; }

        /// <summary>
        /// 交易状态。
        /// SUCCESS 表示支付成功。
        /// </summary>
        public string trade_state { get; set; }

        /// <summary>
        /// 交易状态描述。
        /// 例如：支付成功。
        /// </summary>
        public string trade_state_desc { get; set; }

        /// <summary>
        /// 付款银行。
        /// </summary>
        public string bank_type { get; set; }

        /// <summary>
        /// 支付完成时间。
        /// </summary>
        public DateTime? success_time { get; set; }

        /// <summary>
        /// 付款用户信息。
        /// </summary>
        public WeChatPayNotifyPayerDto payer { get; set; }

        /// <summary>
        /// 支付金额信息。
        /// </summary>
        public WeChatPayNotifyAmountDto amount { get; set; }
    }
}
