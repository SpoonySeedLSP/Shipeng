using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信支付 V3 基础配置。
    /// </summary>
    [SuppressSniffer]
    public class WeChatPayOptions
    {
        /// <summary>
        /// 小程序、公众号或 App 的 AppId。
        /// 注意：这里要和实际发起支付的端一致。
        /// 小程序支付就填小程序 AppId，公众号支付就填公众号 AppId。
        /// </summary>
        public string appId { get; set; }

        /// <summary>
        /// 微信支付商户号。
        /// </summary>
        public string mchId { get; set; }

        /// <summary>
        /// 商户 API 证书序列号。
        /// 注意：
        /// 1. 这是商户证书 apiclient_cert.pem 的序列号。
        /// 2. 不是微信支付平台证书序列号。
        /// 3. 请求微信支付 V3 接口时，Authorization 请求头必须带这个序列号。
        /// </summary>
        public string serialNo { get; set; }

        /// <summary>
        /// 商户 API 私钥文件路径。
        /// 通常是 apiclient_key.pem。
        /// 用途：
        /// 1. 请求微信支付接口时做 RSA-SHA256 签名。
        /// 2. 生成 JSAPI 前端调起支付参数 paySign。
        /// 注意：
        /// 1. 这里填写 apiclient_key.pem。
        /// 2. 代码会用这个私钥生成 RSA 签名。
        /// 3. 不要填写 apiclient_cert.pem。
        /// </summary>
        public string privateKeyPath { get; set; }

        /// <summary>
        /// 微信支付 APIv2 密钥。
        /// 目前 V3 下单、查单、关单用不到。
        /// 后续如果接老版退款、企业付款或部分 V2 接口时可能用到。
        /// </summary>
        public string apiV2Key { get; set; }

        /// <summary>
        /// 微信支付 APIv3 密钥。
        /// 用于解密支付回调、退款回调中的 resource.ciphertext。
        /// 注意：
        /// 1. 必须是 32 字节。
        /// 2. 这是商户平台设置的 APIv3 密钥，不是证书私钥。
        /// </summary>
        public string apiV3Key { get; set; }

        /// <summary>
        /// 微信支付平台证书路径。
        /// 注意：
        /// 1. 这是微信支付平台证书，不是商户 API 证书。
        /// 2. 用于验证微信支付回调签名。
        /// 3. 没有下载平台证书前可以先空着，但正式上线回调必须验签。
        /// </summary>
        public string platformCertificatePath { get; set; }

        /// <summary>
        /// 微信支付回调地址。
        /// 注意：
        /// 1. 这里仅作为下单时传给微信的 notify_url。
        /// 2. 具体回调接口由业务模块实现。
        /// 3. 必须公网 HTTPS 可访问。
        /// </summary>
        public string notifyUrl { get; set; }
    }

}
