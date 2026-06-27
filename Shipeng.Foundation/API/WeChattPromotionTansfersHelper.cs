using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace Shipeng.Util
{
    /// <summary>
    /// 微信支付---企业付款到零钱
    /// </summary>
    public static class WeChattPromotionTansfersHelper
    {
        /// <summary>
        /// 企业付款到零钱
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<WeChatPromotionTansfersDTO> SendNormalRedPackAsync(SendPromotionTansfersDTO input)
        {
            return await Task.Run(() => { return SendNormalRedPack(input); });
        }

        /// <summary>
        /// 查询企业付款到零钱
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<WeChatPromotionTansfersDTO> SendGettransferinfoPackAsync(SendGettransferinfoDTO input)
        {
            return await Task.Run(() => { return SendGettransferinfoPack(input); });
        }

        /// <summary>
        /// 企业付款到零钱
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static WeChatPromotionTansfersDTO SendNormalRedPack(SendPromotionTansfersDTO input)
        {
            if (!File.Exists(input.CertInfoPath))
            {
                return new WeChatPromotionTansfersDTO() { Success = false, Status = 1, StatusText = input.ToJson(), Msg = "微信支付证书不存在" };
            }
            string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";
            string nonce_str = GenerateNonceStr();
            SortedDictionary<string, object> send_values = new SortedDictionary<string, object>
            {
                ["mch_appid"] = input.mch_appid,
                ["mchid"] = input.mchid,
                ["nonce_str"] = nonce_str,
                ["partner_trade_no"] = input.partner_trade_no,
                ["openid"] = input.openid,
                ["check_name"] = input.check_name,
                ["re_user_name"] = input.re_user_name,
                ["amount"] = input.amount,
                ["desc"] = input.desc,
                ["spbill_create_ip"] = input.spbill_create_ip
            };
            string sign = MakeSign(send_values, input.MchKey);
            send_values["sign"] = sign;
            string resData = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.mchid, input.MchKey);
            SortedDictionary<string, object> resultdata = FromXml(resData, input.MchKey);
            LogHelper.WriteLog_WeChartSendredpackTxt("付款返回：" + resultdata.ToJson());
            if (resultdata["return_code"].ToString() == "SUCCESS")//成功
            {
                if (resultdata["result_code"].ToString() == "SUCCESS")//成功
                {
                    WeChatPromotionTansfersDTO returndata = new WeChatPromotionTansfersDTO { Success = true, Status = 0, Msg = "成功" };
                    return returndata;//返回
                }
                else
                {
                    SendGettransferinfoDTO gettransferinfo = new SendGettransferinfoDTO
                    {
                        mch_id = input.mchid,
                        MchKey = input.MchKey,
                        appid = input.mch_appid,
                        partner_trade_no = input.partner_trade_no,
                        CertInfoPath = input.CertInfoPath,
                        openid = input.openid
                    };
                    WeChatPromotionTansfersDTO resDataInfo = SendGettransferinfoPack(gettransferinfo);
                    if (resDataInfo.Success)
                    {
                        return resDataInfo;
                    }
                    else
                    {
                        return new WeChatPromotionTansfersDTO { Success = false, StatusText = "付款失败", Status = 1, Msg = resultdata["err_code_des"].ToString() + "-" + resDataInfo.Msg };
                    }
                }
            }
            else
            {
                WeChatPromotionTansfersDTO returndata = new WeChatPromotionTansfersDTO { Success = false, StatusText = "付款失败", Status = 1, Msg = resultdata["return_msg"].ToString() };
                return returndata;//返回
            }
        }

        /// <summary>
        /// 查询企业付款到零钱
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static WeChatPromotionTansfersDTO SendGettransferinfoPack(SendGettransferinfoDTO input)
        {
            if (!File.Exists(input.CertInfoPath))
            {
                return new WeChatPromotionTansfersDTO() { Success = false, Status = 1, StatusText = input.ToJson(), Msg = "微信支付证书不存在" };
            }
            string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/gettransferinfo";
            string nonce_str = GenerateNonceStr();
            SortedDictionary<string, object> send_values = new SortedDictionary<string, object>
            {
                ["nonce_str"] = nonce_str,
                ["partner_trade_no"] = input.partner_trade_no,
                ["mch_id"] = input.mch_id,
                ["appid"] = input.appid,
                //["openid"] = input.openid,
            };
            string sign = MakeSign(send_values, input.MchKey);
            send_values["sign"] = sign;
            string resData = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.mch_id, input.MchKey);
            SortedDictionary<string, object> resultdata = FromXml(resData, input.MchKey);
            LogHelper.WriteLog_WeChartSendredpackTxt("查询付款返回：" + resultdata.ToJson());
            if (resultdata["return_code"].ToString() == "SUCCESS")//成功
            {
                if (resultdata["result_code"].ToString() == "SUCCESS")//成功
                {
                    string status = resultdata["status"].ToString();
                    if (status == "SUCCESS")
                    {
                        return new WeChatPromotionTansfersDTO { Success = true, StatusText = status, Status = 0, Msg = "领取成功" };
                    }
                    else if (status == "FAILED")
                    {
                        return new WeChatPromotionTansfersDTO { Success = false, StatusText = status, Status = 1, Msg = resultdata["reason"].ToString() };
                    }
                    else
                    {
                        return new WeChatPromotionTansfersDTO { Success = false, StatusText = status, Status = 2, Msg = resultdata["reason"].ToString() };
                    }
                }
                else
                {
                    return new WeChatPromotionTansfersDTO { Success = false, StatusText = "付款失败", Status = 1, Msg = resultdata["err_code_des"].ToString() };
                }
            }
            else
            {
                return new WeChatPromotionTansfersDTO { Success = false, StatusText = "付款失败", Status = 1, Msg = resultdata["return_msg"].ToString() };
            }
        }

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

    public class SendPromotionTansfersDTO
    {
        /// <summary>
        /// 商户账号appid
        /// </summary>
        public string mch_appid { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string mchid { get; set; }

        /// <summary>
        /// 商户订单号
        /// </summary>
        public string partner_trade_no { get; set; }

        /// <summary>
        /// 商户密钥
        /// </summary>
        public string MchKey { get; set; }

        /// <summary>
        /// 证书地址
        /// </summary>
        public string CertInfoPath { get; set; }

        /// <summary>
        /// 用户OpenId
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// 校验用户姓名选项
        /// </summary>
        public string check_name { get; set; } = "NO_CHECK";

        /// <summary>
        /// 收款用户姓名
        /// </summary>
        public string re_user_name { get; set; }

        /// <summary>
        /// 付款金额，单位为分
        /// </summary>
        public int amount { get; set; }

        /// <summary>
        /// 付款备注
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        public string spbill_create_ip { get; set; } = GlobalData.SpbillCreateIp;
    }

    public class SendGettransferinfoDTO
    {
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string partner_trade_no { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string mch_id { get; set; }

        /// <summary>
        /// Appid
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 商户私钥
        /// </summary>
        public string MchKey { get; set; }

        /// <summary>
        /// 证书地址
        /// </summary>
        public string CertInfoPath { get; set; }

        /// <summary>
        /// openid
        /// </summary>
        public string openid { get; set; }
    }

    public class WeChatPromotionTansfersDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 状态  0  领取成功  1 领取失败 2 付款中
        /// </summary>
        public int Status { get; set; }

        public string StatusText { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }
    }
}
