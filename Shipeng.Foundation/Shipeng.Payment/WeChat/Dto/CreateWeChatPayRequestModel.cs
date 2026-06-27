using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 创建微信支付订单请求模型。
    /// </summary>
    [SuppressSniffer]
    public class CreateWeChatPayRequestModel
    {
        /// <summary>
        /// 商户系统内部订单号。
        /// 要求：
        /// 1. 同一个商户号下必须唯一。
        /// 2. 建议使用本系统支付单号，不要直接使用业务订单号。
        /// 3. 一个业务订单如果重新发起支付，可以新建支付单并生成新的 outTradeNo。
        /// </summary>
        public string outTradeNo { get; set; }

        /// <summary>
        /// 商品或订单描述。
        /// 示例：
        /// 古树茶认养订单、农产品商城订单、服务任务保证金。
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// 支付金额，单位：分。
        /// 注意：
        /// 1. 微信支付接口金额单位是分，不是元。
        /// 2. 业务金额如果是 decimal 元，需要在调用前转换成 int 分。
        /// </summary>
        public int totalFee { get; set; }

        /// <summary>
        /// 支付用户 openId。
        /// JSAPI、小程序支付必传。
        /// Native 扫码支付不需要。
        /// </summary>
        public string openId { get; set; }

        /// <summary>
        /// 支付交易类型。
        /// 默认 JSAPI，适合小程序/H5公众号支付。
        /// </summary>
        public WeChatPayTradeTypeEnum tradeType { get; set; } = WeChatPayTradeTypeEnum.JsApi;
    }

}
