using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace Shipeng.Util
{
    /// <summary>
    /// 微信支付---生成付款二维码
    /// </summary>
    public static class WeChatTransactionsNativeHelper
    {
        /// <summary>
        /// 生成付款二维码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<WeChatTransactionsNativeDTO> SendNormalRedPackAsync(SendTransactionsNativeDTO input)
        {
            return await Task.Run(() => { return SendNormalRedPack(input); });
        }

        /// <summary>
        /// 查询生成付款二维码是否付款成功
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<WeChatTransactionsNativeDTO> SendGetTransactionsNativePackAsync(SendGetTransactionsNativeDTO input)
        {
            return await Task.Run(() => { return SendGetTransactionsNativePack(input); });
        }

        /// <summary>
        /// 付款二维码关闭订单
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<WeChatTransactionsNativeDTO> SendCloseorderTransactionsNativePackAsync(SendGetTransactionsNativeDTO input)
        {
            return await Task.Run(() => { return SendCloseorderTransactionsNativePack(input); });
        }

        /// <summary>
        /// 生成付款二维码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static WeChatTransactionsNativeDTO SendNormalRedPack(SendTransactionsNativeDTO input)
        {
            if (!File.Exists(input.CertInfoPath))
            {
                return new WeChatTransactionsNativeDTO() { Success = false, Status = 1, StatusText = input.ToJson(), Msg = "微信支付证书不存在" };
            }
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            string nonce_str = GenerateNonceStr();
            SortedDictionary<string, object> send_values = new SortedDictionary<string, object>
            {
                ["appid"] = input.Appid,
                ["mch_id"] = input.Mchid,
                ["device_info"] = input.Device_info,
                ["nonce_str"] = nonce_str,
                ["sign_type"] = input.Sign_type,
                ["body"] = input.Body,
                ["attach"] = input.Attach,
                ["out_trade_no"] = input.Out_trade_no,
                ["fee_type"] = input.Fee_type,
                ["total_fee"] = input.Total_fee,
                ["spbill_create_ip"] = input.Spbill_create_ip,
                ["time_start"] = input.Time_start,
                ["time_expire"] = input.Time_expire,
                ["notify_url"] = input.Notify_url,
                ["trade_type"] = input.Trade_type,
                ["product_id"] = input.Product_id,
                ["limit_pay"] = input.Limit_pay,
                ["openid"] = input.Openid,
                ["receipt"] = input.Receipt,
                ["profit_sharing"] = input.Profit_sharing
            };
            string sign = MakeSign(send_values, input.MchKey);
            send_values["sign"] = sign;
            string resData = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.Mchid, input.MchKey);
            SortedDictionary<string, object> resultdata = FromXml(resData, input.MchKey);
            LogHelper.WriteLog_WeChartSendredpackTxt("生成付款码返回：" + resultdata.ToJson());
            if (resultdata["return_code"].ToString() == "SUCCESS")//成功
            {
                if (resultdata["result_code"].ToString() == "SUCCESS")//成功
                {
                    WeChatTransactionsNativeDTO returndata = new WeChatTransactionsNativeDTO { Success = true, Status = 0, Msg = "成功" };
                    return returndata;//返回
                }
                else
                {
                    return new WeChatTransactionsNativeDTO { Success = false, StatusText = "生成付款码失败", Status = 1, Msg = resultdata["err_code_des"].ToString() };
                }
            }
            else
            {
                WeChatTransactionsNativeDTO returndata = new WeChatTransactionsNativeDTO { Success = false, StatusText = "生成付款码失败", Status = 1, Msg = resultdata["return_msg"].ToString() };
                return returndata;//返回
            }
        }

        /// <summary>
        /// 查询生成付款二维码是否付款成功
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static WeChatTransactionsNativeDTO SendGetTransactionsNativePack(SendGetTransactionsNativeDTO input)
        {
            if (!File.Exists(input.CertInfoPath))
            {
                return new WeChatTransactionsNativeDTO() { Success = false, Status = 1, StatusText = input.ToJson(), Msg = "微信支付证书不存在" };
            }
            string url = "https://api.mch.weixin.qq.com/pay/orderquery";
            string nonce_str = GenerateNonceStr();
            SortedDictionary<string, object> send_values = new SortedDictionary<string, object>
            {
                ["appid"] = input.Appid,
                ["mch_id"] = input.Mch_id,
                ["nonce_str"] = nonce_str,
                ["out_trade_no"] = input.Out_trade_no,
                ["nonce_str"] = nonce_str,
                ["sign_type"] = input.Sign_type
            };
            string sign = MakeSign(send_values, input.MchKey);
            send_values["sign"] = sign;
            string resData = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.Mch_id, input.MchKey);
            SortedDictionary<string, object> resultdata = FromXml(resData, input.MchKey);
            LogHelper.WriteLog_WeChartSendredpackTxt("付款码--查询付款返回：" + resultdata.ToJson());
            if (resultdata["return_code"].ToString() == "SUCCESS")//成功
            {
                if (resultdata["result_code"].ToString() == "SUCCESS")//成功
                {
                    if (resultdata["trade_state"].ToString() == "SUCCESS")//成功
                    {
                        return new WeChatTransactionsNativeDTO { Success = true, StatusText = "付款码--付款成功", Status = 0, Msg = "付款码--付款成功" };
                    }
                    else if (resultdata["trade_state"].ToString() == "REFUND")//REFUND 转入退款                        
                    {
                        return new WeChatTransactionsNativeDTO { Success = true, StatusText = "付款码--转入退款", Status = 2, Msg = "付款码--转入退款" };
                    }
                    else if (resultdata["trade_state"].ToString() == "NOTPAY")//NOTPAY 未支付                        
                    {
                        return new WeChatTransactionsNativeDTO { Success = true, StatusText = "付款码--未支付", Status = 2, Msg = "付款码--未支付" };
                    }
                    else if (resultdata["trade_state"].ToString() == "CLOSED")//CLOSED 已关闭                        
                    {
                        return new WeChatTransactionsNativeDTO { Success = true, StatusText = "付款码--已关闭", Status = 1, Msg = "付款码--已关闭" };
                    }
                    else if (resultdata["trade_state"].ToString() == "REVOKED")//REVOKED 已撤销(刷卡支付)                        
                    {
                        return new WeChatTransactionsNativeDTO { Success = true, StatusText = "付款码--已撤销", Status = 1, Msg = "付款码--已撤销" };
                    }
                    else if (resultdata["trade_state"].ToString() == "USERPAYING")//USERPAYING 用户支付中                       
                    {
                        return new WeChatTransactionsNativeDTO { Success = true, StatusText = "付款码--用户支付中", Status = 2, Msg = "付款码--用户支付中" };
                    }
                    else if (resultdata["trade_state"].ToString() == "PAYERROR")//REVOKED 支付失败(其他原因，如银行返回失败)                      
                    {
                        return new WeChatTransactionsNativeDTO { Success = true, StatusText = "付款码--支付失败", Status = 1, Msg = "付款码--支付失败" };
                    }
                    else
                    {
                        return new WeChatTransactionsNativeDTO { Success = true, StatusText = "付款码--已接收，等待扣款", Status = 2, Msg = "付款码--已接收，等待扣款" };
                    }
                }
                else
                {
                    return new WeChatTransactionsNativeDTO { Success = false, StatusText = "付款码--付款失败", Status = 1, Msg = resultdata["err_code_des"].ToString() };
                }
            }
            else
            {
                return new WeChatTransactionsNativeDTO { Success = false, StatusText = "付款码--付款失败", Status = 1, Msg = resultdata["return_msg"].ToString() };
            }
        }

        /// <summary>
        /// 付款二维码关闭订单
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static WeChatTransactionsNativeDTO SendCloseorderTransactionsNativePack(SendGetTransactionsNativeDTO input)
        {
            if (!File.Exists(input.CertInfoPath))
            {
                return new WeChatTransactionsNativeDTO() { Success = false, Status = 1, StatusText = input.ToJson(), Msg = "微信支付证书不存在" };
            }
            string url = "https://api.mch.weixin.qq.com/pay/closeorder";
            string nonce_str = GenerateNonceStr();
            SortedDictionary<string, object> send_values = new SortedDictionary<string, object>
            {
                ["appid"] = input.Appid,
                ["mch_id"] = input.Mch_id,
                ["nonce_str"] = nonce_str,
                ["out_trade_no"] = input.Out_trade_no,
                ["nonce_str"] = nonce_str,
                ["sign_type"] = input.Sign_type
            };
            string sign = MakeSign(send_values, input.MchKey);
            send_values["sign"] = sign;
            string resData = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.Mch_id, input.MchKey);
            SortedDictionary<string, object> resultdata = FromXml(resData, input.MchKey);
            LogHelper.WriteLog_WeChartSendredpackTxt("付款码--关闭订单返回：" + resultdata.ToJson());
            if (resultdata["return_code"].ToString() == "SUCCESS")//成功
            {
                if (resultdata["result_code"].ToString() == "SUCCESS")//成功
                {
                    return new WeChatTransactionsNativeDTO { Success = true, StatusText = "付款码--关闭订单", Status = 0, Msg = "付款码--关闭订单" };
                }
                else
                {
                    return new WeChatTransactionsNativeDTO { Success = false, StatusText = "付款码--关闭订单失败", Status = 1, Msg = resultdata["err_code_des"].ToString() };
                }
            }
            else
            {
                return new WeChatTransactionsNativeDTO { Success = false, StatusText = "付款码--关闭订单失败", Status = 1, Msg = resultdata["return_msg"].ToString() };
            }
        }

        /// <summary>
        /// 调用API
        /// </summary>
        /// <param name="send_values"></param>
        /// <param name="url"></param>
        /// <param name="CertInfoPath"></param>
        /// <param name="MchId"></param>
        /// <param name="MchKey"></param>
        /// <returns></returns>
        private static string SendNormalRedPackPost(SortedDictionary<string, object> send_values, string url, string CertInfoPath, string MchId, string MchKey)
        {
            string xml = ToXml(send_values);
            X509Certificate2 cert = X509CertificateLoader.LoadPkcs12FromFile(CertInfoPath, MchId, X509KeyStorageFlags.MachineKeySet);
            return WeChatHttpClientHelper.PostXml(url, xml, cert);
        }

        private static SortedDictionary<string, object> FromXml(string xml, string key)
        {
            SortedDictionary<string, object> result_value = new SortedDictionary<string, object>();
            if (string.IsNullOrEmpty(xml))
            {
                //_logger.LogError(this.GetType().ToString(), "将空的xml串转换为WxPayData不合法!");
                throw new Exception("将空的xml串转换为WxPayData不合法!");
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
            XmlNodeList nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                XmlElement xe = (XmlElement)xn;
                result_value[xe.Name] = xe.InnerText;//获取xml的键值对到WxPayData内部的数据中
            }
            return result_value;
        }

        /// <summary>
        /// 生成随机串，随机串包含字母或数字
        /// </summary>
        /// <returns> </returns>
        private static string GenerateNonceStr()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        /// <summary>
        /// 生成时间戳，标准北京时间，时区为东八区，自1970年1月1日 0点0分0秒以来的秒数
        /// </summary>
        /// <returns> </returns>
        private static string GenerateTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        private static string ToUrl(SortedDictionary<string, object> sign_values)
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in sign_values)
            {
                if (pair.Value == null)
                {
                    //_logger.LogError(this.GetType().ToString(), "WxPayData内部含有值为null的字段!");
                    throw new Exception("WxPayData内部含有值为null的字段!");
                }

                if (pair.Key != "sign" && pair.Value.ToString() != "")
                {
                    buff += pair.Key + "=" + pair.Value + "&";
                }
            }
            buff = buff.Trim('&');
            return buff;
        }

        private static string MakeSign(SortedDictionary<string, object> sign_values, string MchKey)
        {
            //转url格式
            string str = ToUrl(sign_values);
            //在string后加入API KEY
            str += "&key=" + MchKey; //商户key
            //MD5加密
            MD5 md5 = MD5.Create();
            //byte[] bs = Encoding.UTF8.GetBytes(str);
            byte[] bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2"));
            }
            //所有字符转为大写
            return sb.ToString().ToUpper();
        }

        private static string ToXml(SortedDictionary<string, object> sign_values)
        {
            //数据为空时不能转化为xml格式
            if (0 == sign_values.Count)
            {
                //_logger.LogError(this.GetType().ToString(), "WxPayData数据为空!");
                throw new Exception("WxPayData数据为空!");
            }

            string xml = "<xml>";
            foreach (KeyValuePair<string, object> pair in sign_values)
            {
                //字段值不能为null，会影响后续流程
                if (pair.Value == null)
                {
                    //_logger.LogError(this.GetType().ToString(), "WxPayData内部含有值为null的字段!");
                    throw new Exception("WxPayData内部含有值为null的字段!");
                }

                if (pair.Value.GetType() == typeof(int))
                {
                    xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
                }
                else if (pair.Value.GetType() == typeof(string))
                {
                    xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                }
                else//除了string和int类型不能含有其他数据类型
                {
                    //_logger.LogError(this.GetType().ToString(), "WxPayData字段数据类型错误!");
                    throw new Exception("WxPayData字段数据类型错误!");
                }
            }
            xml += "</xml>";
            return xml;
        }

        private static bool IsSet(this SortedDictionary<string, object> sign_values, string key)
        {
            sign_values.TryGetValue(key, out object o);
            if (null != o)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static object GetValue(this SortedDictionary<string, object> sign_values, string key)
        {
            sign_values.TryGetValue(key, out object o);
            return o;
        }

        private static bool CheckSign(SortedDictionary<string, object> sign_values, string key)
        {
            //如果没有设置签名，则跳过检测
            if (!sign_values.IsSet("sign"))
            {
                //_logger.LogError(this.GetType().ToString(), "WxPayData签名存在但不合法!");
                throw new Exception("WxPayData签名不存在!");
            }
            //如果设置了签名但是签名为空，则抛异常
            else if (sign_values.GetValue("sign") == null || sign_values.GetValue("sign").ToString() == "")
            {
                //_logger.LogError(this.GetType().ToString(), "WxPayData签名存在但不合法!");
                throw new Exception("WxPayData签名存在但不合法!");
            }

            //获取接收到的签名
            string return_sign = sign_values.GetValue("sign").ToString();

            //在本地计算新的签名
            string cal_sign = MakeSign(sign_values, key);

            if (cal_sign == return_sign)
            {
                return true;
            }

            //_logger.LogError(this.GetType().ToString(), "WxPayData签名验证错误!");
            throw new Exception("WxPayData签名验证错误!");
        }
    }

    public class SendTransactionsNativeDTO
    {
        /// <summary>
        /// 商户私钥
        /// </summary>
        public string MchKey { get; set; }

        /// <summary>
        /// 证书地址
        /// </summary>
        public string CertInfoPath { get; set; }

        /// <summary>
        /// appid
        /// </summary>
        public string Appid { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string Mchid { get; set; }

        /// <summary>
        /// 设备号
        /// </summary>
        public string Device_info { get; set; } = "WEB";

        /// <summary>
        /// 签名类型
        /// </summary>
        public string Sign_type { get; set; } = "MD5";

        /// <summary>
        /// 标价币种
        /// </summary>
        public string Fee_type { get; set; } = "CNY";

        /// <summary>
        /// 商品描述
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 商品详细描述，对于使用单品优惠的商户，该字段必须按照规范上传，详见
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// 商户订单号
        /// </summary>
        public string Out_trade_no { get; set; }

        /// <summary>
        /// 订单总金额，单位为分
        /// </summary>
        public int Total_fee { get; set; }

        /// <summary>
        /// 终端IP
        /// </summary>
        public string Spbill_create_ip { get; set; } = GlobalData.SpbillCreateIp;

        /// <summary>
        /// 交易起始时间 格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010
        /// </summary>
        public string Time_start { get; set; }

        /// <summary>
        /// 交易结束时间 格式为yyyyMMddHHmmss，如2009年12月27日9点10分10秒表示为20091227091010
        /// </summary>
        public string Time_expire { get; set; }

        /// <summary>
        /// attach  自定义数据  
        /// </summary>
        public string Attach { get; set; }

        /// <summary>
        /// notify_url body 异步接收微信支付结果通知的回调地址，通知url必须为外网可访问的url，不能携带参数。 公网域名必须为https，如果是走专线接入，使用专线NAT IP或者私有回调域名可使用http
        /// </summary>
        public string Notify_url { get; set; }

        /// <summary>
        /// 订单优惠标记
        /// </summary>
        public string Goods_tag { get; set; }

        /// <summary>
        /// 交易类型
        /// </summary>
        public string Trade_type { get; set; } = "NATIVE";

        /// <summary>
        /// trade_type=NATIVE时，此参数必传。此参数为二维码中包含的商品ID，商户自行定义。
        /// </summary>
        public string Product_id { get; set; }

        /// <summary>
        /// 上传此参数no_credit--可限制用户不能使用信用卡支付
        /// </summary>
        public string Limit_pay { get; set; } = "no_credit";

        /// <summary>
        /// openid
        /// </summary>
        public string Openid { get; set; }

        /// <summary>
        /// 电子发票入口开放标识 Y，传入Y时，支付成功消息和支付详情页将出现开票入口。需要在微信支付商户平台或微信公众平台开通电子发票功能，传此字段才可生效
        /// </summary>
        public string Receipt { get; set; } = "N";

        /// <summary>
        /// 是否需要分账 Y-是，需要分账 N-否，不分账 字母要求大写，不传默认不分账
        /// </summary>
        public string Profit_sharing { get; set; } = "N";
    }

    public class SendGetTransactionsNativeDTO
    {
        /// <summary>
        /// 公众账号ID appid
        /// </summary>
        public string Appid { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string Mch_id { get; set; }

        /// <summary>
        /// 商户订单号
        /// </summary>
        public string Out_trade_no { get; set; }

        public string Sign_type { get; set; } = "MD5";

        /// <summary>
        /// 商户私钥
        /// </summary>
        public string MchKey { get; set; }

        /// <summary>
        /// 证书地址
        /// </summary>
        public string CertInfoPath { get; set; }
    }

    public class WeChatTransactionsNativeDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 状态  0  付款成功  1 付款失败 2 付款中
        /// </summary>
        public int Status { get; set; }

        public string StatusText { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }
    }
}
