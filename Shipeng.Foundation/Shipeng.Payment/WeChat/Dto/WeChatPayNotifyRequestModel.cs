using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信支付 V3 回调通知外层模型
    ///
    /// 说明：
    /// 1. 这个模型对应微信支付回调接口收到的原始 JSON。
    /// 2. 真正的支付结果不在外层字段里，而是在 resource.ciphertext 里面。
    /// 3. resource.ciphertext 需要用 APIv3 密钥解密后，才能得到支付订单明文结果。
    /// </summary>
    [SuppressSniffer]
    public class WeChatPayNotifyRequestModel
    {
        /// <summary>
        /// 微信支付通知唯一编号。
        /// 可用于日志记录和排查问题。
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 通知创建时间。
        /// 例如：2024-06-01T12:00:00+08:00。
        /// </summary>
        public DateTime? create_time { get; set; }

        /// <summary>
        /// 通知类型。
        /// 支付成功一般为：TRANSACTION.SUCCESS。
        /// </summary>
        public string event_type { get; set; }

        /// <summary>
        /// 通知资源类型。
        /// 支付成功通知一般为：encrypt-resource。
        /// </summary>
        public string resource_type { get; set; }

        /// <summary>
        /// 加密资源数据。
        /// 微信把真正的支付结果加密放在这里。
        /// </summary>
        public WeChatPayNotifyResourceEncryptDto resource { get; set; }

        /// <summary>
        /// 回调摘要。
        /// 例如：支付成功。
        /// </summary>
        public string summary { get; set; }
    }
}
