using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 创建微信支付订单返回模型。
    /// </summary>
    [SuppressSniffer]
    public class CreateWeChatPayDto
    {
        /// <summary>
        /// 商户系统内部订单号。
        /// 这里原样返回，方便业务层记录或核对。
        /// </summary>
        public string outTradeNo { get; set; }

        /// <summary>
        /// 微信预支付交易会话标识。
        /// JSAPI、小程序支付时微信返回 prepay_id。
        /// </summary>
        public string prepayId { get; set; }

        /// <summary>
        /// Native 扫码支付二维码链接。
        /// 前端拿到后生成二维码，用户用微信扫码支付。
        /// </summary>
        public string codeUrl { get; set; }

        /// <summary>
        /// JSAPI/小程序前端调起支付需要的完整参数。
        /// 前端直接使用这些字段调用 wx.requestPayment。
        /// </summary>
        public WeChatJsApiPayDto jsApiPay { get; set; }
    }

}
