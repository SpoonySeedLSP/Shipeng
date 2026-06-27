using Shipeng.Dependency;

namespace Shipeng.Expand.Thirdparty
{
    /// <summary>
    /// 微信支付常量
    /// </summary>
    [SuppressSniffer]
    public class WechatConstants
    {
        /// <summary>
        /// 微信V3支付密钥
        /// </summary>
        public const string WxAPIV3SecretKey = "";

        /// <summary>
        /// 微信支付证书路径
        /// </summary>
        public static string WxPayCertPath = App.Configuration["WechatOptions:CertPath"];

        /// <summary>
        /// 微信支付Token前缀，使用v3支付api，用curl需要添加请求头，Authorization的类型必须为“WECHATPAY2-SHA256-RSA2048”  [可参考：]
        /// </summary>
        public const string WxPayTokenPrefix = "WECHATPAY2-SHA256-RSA2048";

        /// <summary>
        /// 微信支付加密方式
        /// </summary>
        public const string WxPaySignType = "HMAC-SHA256";

        /// <summary>
        /// 服务商模式 V3支付回調 
        /// </summary>
        public static string WxPayNotifyUrl = "https://portal.one5a.com/WechatPay/Api/V1/FwsV3Pay/WxPayBack";

        /// <summary>
        /// 微信支付Api URL
        /// </summary>
        public static string WxPayUrl = "https://api.mch.weixin.qq.com";

        /// <summary>
        /// 微信支付Api URL V3 jsapi支付 参考官方： https://pay.weixin.qq.com/wiki/doc/apiv3_partner/apis/chapter4_5_1.shtml
        /// </summary>
        public static string WxPayOfJsapiUri = "/v3/pay/partner/transactions/jsapi";
        public static string WxPayOfJsapiUrl = $"{WxPayUrl}{WxPayOfJsapiUri}";

        /// <summary>
        /// URL：https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_4_1.shtml 微信原生支付
        /// </summary>
        public static string WxPayOfNativeUri = "/v3/pay/partner/transactions/native";
        public static string WxPayOfNativeUrl = $"{WxPayUrl}{WxPayOfNativeUri}";

        /// <summary>
        /// 服务商模式：微信支付分账添加分账接收方 
        /// </summary>
        public static string WxPayOfShareAddReceiverUri = "/pay/profitsharingaddreceiver";
        public static string WxPayOfShareAddReceiverUrl = $"{WxPayUrl}{WxPayOfShareAddReceiverUri}";

        /// <summary>
        /// 微信分账接口
        /// </summary>
        public static string WxPayOfShareProfitUri = "/secapi/pay/profitsharing";
        public static string WxPayOfShareProfitUrl = $"{WxPayUrl}{WxPayOfShareProfitUri}";

        /// <summary>
        /// 对应微信地址：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_1
        /// </summary>
        public static string WxPayOfUnifiedUrl = $"{WxPayUrl}/pay/unifiedorder";

        /// <summary>
        /// 写在配置文件内的AppID是小程序APPID --有商户平台关联
        /// </summary>
        public static string CustomerAppId = "";

        /// <summary>
        /// 写在配置文件内的MchId是商户的 MchId 
        /// </summary>
        public static string CustomerMchId = "";

        /// <summary>
        /// 商户平台Api密钥
        /// </summary>
        public static string CustomerKey = "";

        /// <summary>
        /// 微商城退款需要证书，证书路径 
        /// </summary>
        public static string WxUnifiedPayCertPath = @"xxx\apiclient_cert.p12";

        /// <summary>
        /// 微商城统一下单支付回调URL
        /// </summary>
        public static string WxUnifiedPayNotifyUrl = "https://portal.one5a.com/WechatPay/Api/V1/Pay/WxPayBack";

        /// <summary>
        /// 微信支付退款
        /// </summary>
        public static string WxUnifiedRefundNotifyUrl = "https://portal.one5a.com/WechatPay/Api/V1/Pay/WxRefundBack";//todo
        public static string WxUnifiedPayOfRefundUrl = $"{WxPayUrl}/secapi/pay/refund";
        public static string WxPayOfRefundUri = "/v3/refund/domestic/refunds";
        public static string WxPayOfRefundUrl = $"{WxPayUrl}{WxPayOfRefundUri}";
        public static string WxRefundNotifyUrl = "https://portal.one5a.com/WechatPay/Api/V1/FwsV3Pay/RefundBack";

        /// <summary>
        /// 微信支付附加信息
        /// </summary>
        public const string OMPayAttach = "";
        public const string OMPayBody = "小程序支付，商城购买";

        /// <summary>
        /// 微信子商户相关参数，商户名称
        /// </summary>
        public const string MerchantName_2 = "";

        /// <summary>
        /// 微信支付服务商模式 子商户商户ID
        /// </summary>
        public const string MerchantId_2 = "";

        /// <summary>
        /// 微信支付 服务商商户名称
        /// </summary>
        public const string MerchantName = "";

        /// <summary>
        /// 微信支付 服务商模式子商户商户ID
        /// </summary>
        public const string MerchantId = "";

        /// <summary>
        /// 微信支付证书序列号
        /// </summary>
        public const string MerchantSerialNo = "66D729C81331C58AFDF7B7470108403C376B9890";

        /// <summary>
        /// 小程序APPID 
        /// </summary>
        public const string MerchantAppId = "";

        /// <summary>
        /// 小程序APP KEY
        /// </summary>
        public const string MerchantAppKey = "";

        /// <summary>
        /// 私钥
        /// </summary>
        public const string MerchantPrivateKey = @"";


        #region 子商户信息
        /// <summary>
        /// 子商户信息 分账接收方
        /// </summary>
        public static string zsfwsMchName = "";
        public static string fwsMchId = ""; //服务商商户ID   
        public static string fwsAppId = ""; //服务器小程序APPID  
        public static string fwsAppSecret = "";//App密钥 
        #endregion


    }
}
