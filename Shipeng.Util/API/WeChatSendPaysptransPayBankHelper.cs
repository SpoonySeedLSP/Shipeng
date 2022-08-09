using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Shipeng.Util
{
    /// <summary>
    /// 微信支付---企业付款到银行卡
    /// </summary>
    public static class WeChatSendPaysptransPayBankHelper
    {
        /// <summary>
        /// 企业付款到银行卡
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<WeChatPromotionPayBankDTO> SendNormalRedPackAsync(SendPromotionPayBankDTO input)
        {
            return await Task.Run(() => { return SendNormalRedPack(input); });
        }

        /// <summary>
        /// 查询企业付款到银行卡
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<WeChatPromotionPayBankDTO> SendGetRedPackinfoPackAsync(SendPromotionPayBankDTO input)
        {
            return await Task.Run(() => { return SendGetRedPackinfoPack(input); });
        }

        /// <summary>
        /// 企业付款到银行卡
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static WeChatPromotionPayBankDTO SendNormalRedPack(SendPromotionPayBankDTO input)
        {
            if (!File.Exists(input.CertInfoPath))
            {
                return new WeChatPromotionPayBankDTO() { Success = false, Status = 1, StatusText = input.ToJson(), Msg = "微信支付证书不存在" };
            }
            string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";
            string nonce_str = GenerateNonceStr();
            WeChatPromotionPayBankDTO keydata = GetPublickey(input, nonce_str);

            if (!keydata.Success)
            {
                return keydata;
            }
            else
            {
                string enc_bank_no = RSAEncrypt(keydata.StatusText, input.enc_bank_no);
                string enc_true_name = RSAEncrypt(keydata.StatusText, input.enc_true_name);
                SortedDictionary<string, object> send_values = new SortedDictionary<string, object>
                {
                    ["mch_appid"] = input.appid,
                    ["mchid"] = input.mch_id,
                    ["nonce_str"] = nonce_str,
                    ["partner_trade_no"] = input.partner_trade_no,
                    ["enc_bank_no"] = enc_bank_no,
                    ["enc_true_name"] = enc_true_name,
                    ["bank_code"] = input.bank_code,
                    ["amount"] = input.amount,
                    ["desc"] = input.desc
                };
                string sign = MakeSign(send_values, input.MchKey);
                send_values["sign"] = sign;
                string resData = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.mch_id, input.MchKey);
                SortedDictionary<string, object> resultdata = FromXml(resData, input.MchKey);
                LogHelper.WriteLog_WeChartSendredpackTxt("付款银行卡返回：" + resultdata.ToJson());
                if (resultdata["return_code"].ToString() == "SUCCESS")//成功
                {
                    if (resultdata["result_code"].ToString() == "SUCCESS")//成功
                    {
                        WeChatPromotionPayBankDTO returndata = new WeChatPromotionPayBankDTO { Success = true, Status = 0, Msg = "成功" };
                        return returndata;//返回
                    }
                    else
                    {
                        SendPromotionPayBankDTO gettransferinfo = new SendPromotionPayBankDTO
                        {
                            mch_id = input.mch_id,
                            MchKey = input.MchKey,
                            appid = input.appid,
                            partner_trade_no = input.partner_trade_no,
                            CertInfoPath = input.CertInfoPath,
                            openid = input.openid
                        };
                        WeChatPromotionPayBankDTO resDataInfo = SendGetRedPackinfoPack(gettransferinfo);
                        if (resDataInfo.Success)
                        {
                            return resDataInfo;
                        }
                        else
                        {
                            return new WeChatPromotionPayBankDTO { Success = false, StatusText = "付款失败", Status = 1, Msg = resultdata["err_code_des"].ToString() + "-" + resDataInfo.Msg };
                        }
                    }
                }
                else
                {
                    WeChatPromotionPayBankDTO returndata = new WeChatPromotionPayBankDTO { Success = false, StatusText = "付款失败", Status = 1, Msg = resultdata["return_msg"].ToString() };
                    return returndata;//返回
                }
            }
        }

        /// <summary>
        /// 获取公钥
        /// </summary>
        /// <param name="input"></param>
        /// <param name="nonce_str"></param>
        /// <returns></returns>
        public static WeChatPromotionPayBankDTO GetPublickey(SendPromotionPayBankDTO input, string nonce_str)
        {
            string str = string.Empty;
            string url = "https://fraud.mch.weixin.qq.com/risk/getpublickey";
            SortedDictionary<string, object> send_values = new SortedDictionary<string, object>
            {
                ["mch_id"] = input.mch_id,
                ["nonce_str"] = nonce_str,
                ["sign_type"] = "MD5"
            };
            string sign = MakeSign(send_values, input.MchKey);
            send_values["sign"] = sign;
            string resData = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.mch_id, input.MchKey);
            SortedDictionary<string, object> resultdata = FromXml(resData, input.MchKey);
            LogHelper.WriteLog_WeChartSendredpackTxt("获取公钥：" + resultdata.ToJson());
            if (resultdata["return_code"].ToString() == "SUCCESS")
            {
                if (resultdata["result_code"].ToString() == "SUCCESS")//成功
                {
                    string publickey = resultdata["pub_key"].ToString();
                    string publickeyreplace = publickey.Replace("-----BEGIN RSA PUBLIC KEY-----", "").Replace("-----END RSA PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "").Trim();
                    publickeyreplace = "-----BEGIN PUBLIC KEY-----\n" + "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A" + sign + "\r-----END PUBLIC KEY-----";
                    return new WeChatPromotionPayBankDTO { Success = true, StatusText = publickeyreplace, Status = 0, Msg = "获取公钥成功" };
                }
                else
                {
                    return new WeChatPromotionPayBankDTO { Success = false, StatusText = "获取公钥失败", Status = 1, Msg = resultdata["err_code_des"].ToString() };
                }
            }
            else
            {
                return new WeChatPromotionPayBankDTO { Success = false, StatusText = "获取公钥失败", Status = 1, Msg = resultdata["return_msg"].ToString() };
            }
        }

        public static string RSAEncrypt(string publickey, string content)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(publickey);
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);

            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// 查询企业付款到银行卡
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static WeChatPromotionPayBankDTO SendGetRedPackinfoPack(SendPromotionPayBankDTO input)
        {
            if (!File.Exists(input.CertInfoPath))
            {
                return new WeChatPromotionPayBankDTO() { Success = false, Status = 1, StatusText = input.ToJson(), Msg = "微信支付证书不存在" };
            }
            string url = "https://api.mch.weixin.qq.com/mmpaysptrans/query_bank";
            string nonce_str = GenerateNonceStr();
            SortedDictionary<string, object> send_values = new SortedDictionary<string, object>
            {
                ["mch_id"] = input.mch_id,
                ["partner_trade_no"] = input.partner_trade_no,
                ["nonce_str"] = nonce_str
            };
            string sign = MakeSign(send_values, input.MchKey);
            send_values["sign"] = sign;
            string resData = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.mch_id, input.MchKey);
            SortedDictionary<string, object> resultdata = FromXml(resData, input.MchKey);
            LogHelper.WriteLog_WeChartSendredpackTxt("查询付款银行卡返回：" + resultdata.ToJson());
            if (resultdata["return_code"].ToString() == "SUCCESS")//成功
            {
                if (resultdata["result_code"].ToString() == "SUCCESS")//成功
                {
                    string status = resultdata["status"].ToString();
                    if (status == "SUCCESS")
                    {
                        return new WeChatPromotionPayBankDTO { Success = true, StatusText = status, Status = 0, Msg = "领取成功" };
                    }
                    else if (status == "FAILED")
                    {
                        return new WeChatPromotionPayBankDTO { Success = false, StatusText = status, Status = 1, Msg = resultdata["reason"].ToString() };
                    }
                    else if (status == "PROCESSING")
                    {
                        return new WeChatPromotionPayBankDTO { Success = false, StatusText = status, Status = 2, Msg = resultdata["reason"].ToString() };
                    }
                    else
                    {
                        return new WeChatPromotionPayBankDTO { Success = false, StatusText = status, Status = 3, Msg = resultdata["reason"].ToString() };
                    }
                }
                else
                {
                    return new WeChatPromotionPayBankDTO { Success = false, StatusText = "付款失败", Status = 1, Msg = resultdata["err_code_des"].ToString() };
                }
            }
            else
            {
                return new WeChatPromotionPayBankDTO { Success = false, StatusText = "付款失败", Status = 1, Msg = resultdata["return_msg"].ToString() };
            }
        }

        private static string SendNormalRedPackPost(SortedDictionary<string, object> send_values, string url, string CertInfoPath, string MchId, string MchKey)
        {
            string xml = ToXml(send_values);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            X509Certificate2 cert = new X509Certificate2(CertInfoPath, MchId, X509KeyStorageFlags.MachineKeySet);
            request.ClientCertificates.Add(cert);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            byte[] data = Encoding.UTF8.GetBytes(xml);
            request.ContentLength = data.Length;
            using Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);

            using HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            int httpStatusCode = (int)response.StatusCode;
            using Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            string resData = reader.ReadToEnd();

            return resData;
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

    public class SendPromotionPayBankDTO
    {
        /// <summary>
        /// 商户账号appid
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string mch_id { get; set; }

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
        /// 收款银行账号
        /// </summary>
        public string enc_bank_no { get; set; }

        /// <summary>
        /// 收款用户姓名
        /// </summary>
        public string enc_true_name { get; set; }


        /// <summary>
        /// 收款方开户行
        /// </summary>
        public string bank_code { get; set; }


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

    public class WeChatPromotionPayBankDTO
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
