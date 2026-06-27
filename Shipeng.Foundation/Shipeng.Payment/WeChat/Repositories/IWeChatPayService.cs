namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信支付 V3 基础封装接口。
    ///
    /// 注意：
    /// 这里只定义微信支付基础能力，不定义认养订单、商城订单、服务订单等业务方法。
    /// </summary>
    public interface IWeChatPayService
    {
        /// <summary>
        /// 创建微信支付订单。
        /// JSAPI/小程序支付返回 prepayId 和前端调起支付参数；
        /// Native 扫码支付返回 codeUrl。
        /// </summary>
        Task<CreateWeChatPayDto> CreatePayAsync(CreateWeChatPayRequestModel input);

        /// <summary>
        /// 根据商户订单号查询微信支付订单。
        /// 本地没有收到回调、回调异常、对账补偿时使用。
        /// </summary>
        Task<string> QueryByOutTradeNoAsync(string outTradeNo);

        /// <summary>
        /// 根据微信支付订单号查询微信支付订单。
        /// 已经拿到 transaction_id 后，可用这个接口查询微信侧交易状态。
        /// </summary>
        Task<string> QueryByTransactionIdAsync(string transactionId);

        /// <summary>
        /// 关闭未支付的微信支付订单。
        /// 已支付订单不能关单，已支付要走退款。
        /// </summary>
        Task CloseAsync(string outTradeNo);

        /// <summary>
        /// 申请退款。
        /// 业务模块校验订单可退款后，调用这里请求微信退款。
        /// </summary>
        Task<string> RefundAsync(CreateWeChatRefundRequestModel input);

        /// <summary>
        /// 根据商户退款单号查询退款状态。
        /// 用于退款回调未收到、退款状态补偿、人工排查。
        /// </summary>
        Task<string> QueryRefundByOutRefundNoAsync(string outRefundNo);

        /// <summary>
        /// 解析微信支付成功回调。
        /// 只负责验签、解密、反序列化，不处理业务订单。
        /// </summary>
        WeChatPayNotifyResourceDto ParsePayNotify(IDictionary<string, string> headers, string body);

        /// <summary>
        /// 解析微信退款回调。
        /// 只负责验签、解密、反序列化，不处理业务退款单。
        /// </summary>
        WeChatRefundNotifyResourceDto ParseRefundNotify(IDictionary<string, string> headers, string body);

        /// <summary>
        /// 验证微信支付/退款回调签名。
        /// 使用微信支付平台证书公钥，不是商户 API 证书。
        /// </summary>
        bool VerifyNotifySignature(string timestamp, string nonce, string body, string signature);

        /// <summary>
        /// 解密微信支付/退款回调 resource。
        /// 使用 APIv3 密钥 AES-256-GCM 解密。
        /// </summary>
        string DecryptNotifyResource(string associatedData, string nonce, string ciphertext);

        /// <summary>
        /// 构建微信回调成功响应。
        /// 业务接口处理成功后直接返回这个对象。
        /// </summary>
        object BuildNotifySuccess();

        /// <summary>
        /// 构建微信回调失败响应。
        /// 返回 FAIL 后微信会重试通知。
        /// </summary>
        object BuildNotifyFail(string message);
    }
}