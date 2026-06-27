namespace Shipeng.Util
{
    /// <summary>
    /// 打赏支付
    /// </summary>
    public class WeChatPayPubPay
    {
        /// <summary>
        /// 微信应用号(公众平台AppId/开放平台AppId/小程序AppId/企业微信CorpId)
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 微信支付 商户号
        /// </summary>
        public string MchId { get; set; }

        /// <summary>
        /// 经纪人id
        /// </summary>
        public string AgentId { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 商品描述(例如腾讯充值中心-QQ会员充值)
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal PayMoney { get; set; }
    }

    /// <summary>
    /// 商品支付
    /// </summary>
    public class WeChatPayProductPay
    {
        /// <summary>
        /// 微信应用号(公众平台AppId/开放平台AppId/小程序AppId/企业微信CorpId)
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 微信支付 商户号
        /// </summary>
        public string MchId { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 商品描述(例如腾讯充值中心-QQ会员充值)
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 订单id
        /// </summary>
        public string OrderId { get; set; }
    }
}
