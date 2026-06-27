using Shipeng.Dependency;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信支付回调 resource 加密数据。
    ///
    /// 说明：
    /// 1. ciphertext 是密文。
    /// 2. associated_data 和 nonce 是解密必须参数。
    /// 3. 解密算法是 AEAD_AES_256_GCM。
    /// </summary>
    [SuppressSniffer]
    public class WeChatPayNotifyResourceEncryptDto
    {
        /// <summary>
        /// 加密算法。
        /// 一般为：AEAD_AES_256_GCM。
        /// </summary>
        public string algorithm { get; set; }

        /// <summary>
        /// 加密数据。
        /// 需要使用 APIv3 密钥解密。
        /// </summary>
        public string ciphertext { get; set; }

        /// <summary>
        /// 附加数据。
        /// AES-GCM 解密时必须原样传入。
        /// </summary>
        public string associated_data { get; set; }

        /// <summary>
        /// 随机串。
        /// AES-GCM 解密时必须原样传入。
        /// </summary>
        public string nonce { get; set; }

        /// <summary>
        /// 原始回调类型。
        /// 支付回调一般为：transaction。
        /// </summary>
        public string original_type { get; set; }
    }
}
