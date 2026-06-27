using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 创建微信退款请求模型。
    ///
    /// 说明：
    /// 1. 这个模型只用于请求微信支付退款接口。
    /// 2. 是否允许退款、退款金额是否正确、订单是否已支付，由业务模块先校验。
    /// 3. 本模型里的金额单位全部是“分”，不是“元”。
    /// 4. 商户退款单号 outRefundNo 必须在本系统内唯一，建议使用本地退款单号。
    /// </summary>
    [SuppressSniffer]
    public class CreateWeChatRefundRequestModel
    {
        /// <summary>
        /// 商户订单号。
        /// 这是创建微信支付订单时传给微信的 out_trade_no。
        ///
        /// 使用规则：
        /// 1. 如果 transactionId 为空，则必须传 outTradeNo。
        /// 2. 如果 transactionId 和 outTradeNo 都传，微信优先使用 transactionId。
        /// </summary>
        public string outTradeNo { get; set; }

        /// <summary>
        /// 微信支付订单号。
        /// 支付成功后微信返回的 transaction_id。
        ///
        /// 使用规则：
        /// 1. 推荐优先使用 transactionId 退款。
        /// 2. 如果没有保存 transactionId，也可以使用 outTradeNo 退款。
        /// </summary>
        public string transactionId { get; set; }

        /// <summary>
        /// 商户退款单号。
        ///
        /// 要求：
        /// 1. 必填。
        /// 2. 同一个商户号下必须唯一。
        /// 3. 建议使用本系统退款单号。
        /// 4. 后续查询退款状态、处理退款回调，都要用到这个编号。
        /// </summary>
        public string outRefundNo { get; set; }

        /// <summary>
        /// 退款原因。
        ///
        /// 示例：
        /// 用户取消订单、商户缺货、订单超时、协商退款。
        /// </summary>
        public string reason { get; set; }

        /// <summary>
        /// 退款金额，单位：分。
        ///
        /// 注意：
        /// 1. 必须大于0。
        /// 2. 不能大于原订单金额 totalFee。
        /// 3. 部分退款时，这里填写本次退款金额。
        /// </summary>
        public int refundFee { get; set; }

        /// <summary>
        /// 原订单金额，单位：分。
        ///
        /// 注意：
        /// 1. 这里是原支付订单总金额。
        /// 2. 不是剩余可退金额。
        /// 3. 微信退款接口要求传原订单总金额。
        /// </summary>
        public int totalFee { get; set; }

        /// <summary>
        /// 退款结果回调地址。
        ///
        /// 说明：
        /// 1. 可为空。
        /// 2. 如果为空，微信可能不单独通知退款结果，业务系统需要主动查询退款状态。
        /// 3. 建议业务模块传自己的退款回调地址，例如：
        ///    https://ywsy315.com/api/payment/refundnotify
        /// 4. 这里不建议写死到微信支付封装层，因为不同业务可能有不同的退款处理逻辑。
        /// </summary>
        public string notifyUrl { get; set; }
    }
}