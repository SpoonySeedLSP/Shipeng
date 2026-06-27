using Shipeng.Util.Helper;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace Shipeng.Util
{
    /// <summary>
    /// 微信支付---小程序红包
    /// </summary>
    public static class WeChatSendredpackHelper
    {
        /// <summary>
        /// 发红包
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<WeChatSendredpackDTO> SendNormalRedPackAsync(SendRedpackDTO input)
        {
            return await Task.Run(() => { return SendNormalRedPack(input); });
        }

        /// <summary>
        /// 发红包返回值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static WeChatSendredpackDTO SendNormalRedPack(SendRedpackDTO input)
        {
            if (!File.Exists(input.CertInfoPath))
            {
                return new WeChatSendredpackDTO() { Success = false, Msg = "微信支付证书不存在" };
            }
            string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/sendminiprogramhb";
            string nonce_str = GenerateNonceStr();
            SortedDictionary<string, object> send_values = new SortedDictionary<string, object>
            {
                ["nonce_str"] = nonce_str,
                ["mch_billno"] = input.MchBillno,
                ["mch_id"] = input.MchId,
                ["wxappid"] = input.AppId,
                ["send_name"] = input.SendName,
                ["re_openid"] = input.OpenId,
                ["total_amount"] = input.HBMoney,
                ["total_num"] = 1,
                ["wishing"] = input.Wishing,
                ["act_name"] = input.ActName,
                ["remark"] = input.Remark,
                ["notify_way"] = input.NonceStr,
                ["scene_id"] = input.SceneId,
                ["client_ip"] = GlobalData.SpbillCreateIp
            };
            string sign = MakeSign(send_values, input.MchKey);
            send_values["sign"] = sign;
            string resData = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.MchId, input.MchKey);
            SortedDictionary<string, object> resultdata = FromXml(resData, input.MchKey);
            LogHelper.WriteLog_WeChartSendredpackTxt(resultdata.ToJson());
            if (resultdata["return_code"].ToString() == "SUCCESS")//成功
            {
                if (resultdata["result_code"].ToString() == "SUCCESS")//成功
                {
                    string timestamp = GenerateTimeStamp();
                    string noncestr = GenerateNonceStr();
                    string package = WebUtility.UrlEncode(resultdata["package"].ToString());
                    SortedDictionary<string, object> sendbizRedPacket_value = new SortedDictionary<string, object>
                    {
                        ["appId"] = input.AppId,
                        ["timeStamp"] = timestamp,
                        ["nonceStr"] = noncestr,
                        ["package"] = package
                    };
                    string sendsign = MakeSign(sendbizRedPacket_value, input.MchKey);
                    WeChatSendredpackDTO returndata = new WeChatSendredpackDTO
                    {
                        TimeStamp = timestamp,
                        NonceStr = noncestr,
                        Package = package,
                        SignType = WxPayData.SIGN_TYPE_MD5,
                        PaySign = sendsign,
                        Success = true
                    };
                    return returndata;//返回
                }
                else
                {
                    string resData1 = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.MchId, input.MchKey);
                    SortedDictionary<string, object> resultdata1 = FromXml(resData1, input.MchKey);
                    LogHelper.WriteLog_WeChartSendredpackTxt(resultdata1.ToJson());
                    if (resultdata1["return_code"].ToString() == "SUCCESS")
                    {
                        if (resultdata1["result_code"].ToString() == "SUCCESS")
                        {
                            string timestamp = GenerateTimeStamp();
                            string noncestr = GenerateNonceStr();
                            string package = WebUtility.UrlEncode(resultdata1["package"].ToString());
                            SortedDictionary<string, object> sendbizRedPacket_value = new SortedDictionary<string, object>
                            {
                                ["appId"] = input.AppId,
                                ["timeStamp"] = timestamp,
                                ["nonceStr"] = noncestr,
                                ["package"] = package,
                            };
                            string sendsign = MakeSign(sendbizRedPacket_value, input.MchKey);
                            WeChatSendredpackDTO returndata = new WeChatSendredpackDTO
                            {
                                TimeStamp = timestamp,
                                NonceStr = noncestr,
                                Package = package,
                                SignType = WxPayData.SIGN_TYPE_MD5,
                                PaySign = sendsign,
                                Success = true
                            };
                            return returndata;//返回
                        }
                        else
                        {
                            return new WeChatSendredpackDTO { Success = false, Msg = resultdata1["return_msg"].ToString() };
                        }
                    }
                    else
                    {
                        return new WeChatSendredpackDTO { Success = false, Msg = resultdata1["return_msg"].ToString() };
                    }
                }
            }
            else
            {
                string resData1 = SendNormalRedPackPost(send_values, url, input.CertInfoPath, input.MchId, input.MchKey);
                SortedDictionary<string, object> resultdata1 = FromXml(resData1, input.MchKey);
                LogHelper.WriteLog_WeChartSendredpackTxt(resultdata1.ToJson());
                if (resultdata1["return_code"].ToString() == "SUCCESS")
                {
                    if (resultdata1["result_code"].ToString() == "SUCCESS")
                    {
                        string timestamp = GenerateTimeStamp();
                        string noncestr = GenerateNonceStr();
                        string package = WebUtility.UrlEncode(resultdata1["package"].ToString());
                        SortedDictionary<string, object> sendbizRedPacket_value = new SortedDictionary<string, object>
                        {
                            ["appId"] = input.AppId,
                            ["timeStamp"] = timestamp,
                            ["nonceStr"] = noncestr,
                            ["package"] = package,
                        };
                        string sendsign = MakeSign(sendbizRedPacket_value, input.MchKey);
                        WeChatSendredpackDTO returndata = new WeChatSendredpackDTO
                        {
                            TimeStamp = timestamp,
                            NonceStr = noncestr,
                            Package = package,
                            SignType = WxPayData.SIGN_TYPE_MD5,
                            PaySign = sendsign,
                            Success = true
                        };
                        return returndata;//返回
                    }
                    else
                    {
                        return new WeChatSendredpackDTO { Success = false, Msg = resultdata1["return_msg"].ToString() };
                    }
                }
                else
                {
                    return new WeChatSendredpackDTO { Success = false, Msg = resultdata1["return_msg"].ToString() };
                }
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

    public class SendRedpackDTO
    {
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string MchBillno { get; set; }
        /// <summary>
        /// 小程序id
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 商户号id
        /// </summary>
        public string MchId { get; set; }

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
        public string OpenId { get; set; }
        /// <summary>
        /// 红包发送者名称  商户名称
        /// </summary>
        public string SendName { get; set; }

        /// <summary>
        /// 红包金额(单位是分)
        /// </summary>
        public int HBMoney { get; set; }

        /// <summary>
        /// 红包祝福语
        /// </summary>
        public string Wishing { get; set; }

        /// <summary>
        /// 活动名称
        /// </summary>
        public string ActName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 通知用户形式
        /// </summary>
        public string NonceStr { get; set; } = "MINI_PROGRAM_JSAPI";

        /// <summary>
        /// 发放红包使用场景，红包金额大于200时必传 PRODUCT_1:商品促销 PRODUCT_2:抽奖 PRODUCT_3:虚拟物品兑奖 PRODUCT_4:企业内部福利 PRODUCT_5:渠道分润 PRODUCT_6:保险回馈 PRODUCT_7:彩票派奖 PRODUCT_8:税务刮奖
        /// </summary>
        public string SceneId { get; set; }
    }

    public class WeChatSendredpackDTO
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public string TimeStamp { get; set; }

        /// <summary>
        /// 随机字符串
        /// </summary>
        public string NonceStr { get; set; }

        /// <summary>
        /// 红包详情的扩展Package
        /// </summary>
        public string Package { get; set; }

        /// <summary>
        /// 签名方式
        /// </summary>
        public string SignType { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string PaySign { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }
    }
}
