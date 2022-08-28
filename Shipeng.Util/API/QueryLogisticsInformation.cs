using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Shipeng.Util
{
    /// <summary>
    /// 通过物流编号查询物流信息
    /// </summary>
    public static class QueryLogisticsInformation
    {
        #region 快递100
        //#region 你的customer与key值
        private static readonly string Customer = "57137879254E88D2624D8B95830761EC";
        private static readonly string Key = "HvgGjKnQ4964";
        //#endregion
        public static async Task<MessageModel<TestDateDTO>> QueryInformationByKD100(string number = "YT4646876453657")
        {
            return await Task.Run(() =>
            {
                return QueryByKD100(number);
            });
        }

        private static MessageModel<TestDateDTO> QueryByKD100(string number)
        {
            string res = string.Empty;
            string Msg = string.Empty;
            bool Success = false;
            TestDateDTO RequestList = new TestDateDTO();
            if (!string.IsNullOrEmpty(number))
            {
                Dictionary<string, object> numparamters = new Dictionary<string, object>
            {
                { "text", number }
            };
                string queryurl = "http://www.kuaidi100.com/autonumber/autoComNum";
                string autocomnum = HttpHelper.GetData(queryurl, numparamters);
                ExpressKD100DTO express = JsonConvert.DeserializeObject<ExpressKD100DTO>(autocomnum);
                if (express.Auto.Any())
                {
                    foreach (SingleDTO autocomnumdetail in express.Auto)
                    {
                        string code = autocomnumdetail.ComCode;
                        ParamDTO p = new ParamDTO
                        {
                            Com = code,
                            Num = number
                        }; string param = JsonConvert.SerializeObject(p);

                        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();                //将参数转为字节数组
                        byte[] InBytes = Encoding.GetEncoding("UTF-8").GetBytes(param + Key + Customer);
                        #region 加密
                        byte[] OutBytes = md5.ComputeHash(InBytes);
                        string OutString = "";
                        for (int i = 0; i < OutBytes.Length; i++)
                        {
                            OutString += OutBytes[i].ToString("x2");
                        }
                        #endregion
                        string sign = OutString.ToUpper();
                        //http://poll.kuaidi100.com/poll/query.do?customer=57137879254E88D2624D8B95830761EC&sign=1613131FDA5CE20E7BE5CD23B67E5DB8&param={%22com%22:%22yunda%22,%22num%22:%22SF1028123140603%22,%22from%22:%22%22,%22phone%220182%22%22,%22to%22:%22%22,%22resultv2%22:%220%22,%22show%22:%220%22,%22order%22:%22desc%22}
                        Dictionary<string, object> paramters = new Dictionary<string, object>
                    {
                           { "customer", Customer },
                           { "sign", sign },
                           { "param", param }
                    };
                        string posturl = "http://poll.kuaidi100.com/poll/query.do";
                        string data = HttpHelper.PostData(posturl, paramters);
                        ResponseDataDto list = JsonConvert.DeserializeObject<ResponseDataDto>(data);
                        if (list.Result)
                        {
                            if (list.Data.Any())
                            {
                                Msg = "物流信息已获取";
                                Success = true;
                                List<HttpDataDTO> rList = new List<HttpDataDTO>();
                                list.Data.ForEach((m) =>
                                {
                                    rList.Add(new HttpDataDTO { CreateDate = m.Time, JobName = m.Ftime, JobOther = m.Context });
                                });
                                RequestList = new TestDateDTO() { Data = rList };
                            }
                            else
                            {
                                Msg = list.Message;
                                Success = false;
                            }
                        }
                        else
                        {
                            Msg = "单号有误";
                            Success = false;
                        }
                    }
                }
                else
                {
                    Msg = "单号有误";
                    Success = false;
                }
            }
            else
            {
                Msg = "单号有误";
                Success = false;
            }
            return new MessageModel<TestDateDTO>
            {
                Msg = Msg,
                Success = Success,
                Response = RequestList
            };
        }
        #endregion

        #region 快递鸟接口
        //电商ID
        private static readonly string EBusinessID = "1660254";
        //电商加密私钥，快递鸟提供，注意保管，不要泄漏
        private static readonly string AppKey = "61c7b1fa-bee5-47a5-a62b-9af05dcfcfee";
        //测试请求url
        private static readonly string ReqURL = "http://api.kdniao.com/Ebusiness/EbusinessOrderHandle.aspx";
        //#endregion
        public static async Task<OrderTracesData> QueryInformationByKDN(string number, string phone = null)
        {
            if (number.IsNullOrEmpty())
            {
                return new OrderTracesData
                {
                    LogisticCode = number,
                    State = 400,
                    Success = false,
                    Traces = null,
                    Reason = "快递单号不能为空，请输入正确的快递单号"
                };
            }
            string regex = "((?=[\x21-\x7e]+)[^A-Za-z0-9])";
            Regex reg = new Regex(regex);
            bool isok = reg.IsMatch(number);
            if (number.Contains("11111") || number.Contains("123456") || isok)
            {
                return new OrderTracesData
                {
                    LogisticCode = number,
                    State = 400,
                    Success = false,
                    Traces = null,
                    Reason = "快递单号错误，请输入正确的快递单号"
                };
            }
            return await Task.Run(() =>
            {
                return OrderTraces(number, phone);
            });
        }

        public static OrderTracesData OrderTraces(string number, string phone = null)
        {
            number = number.Trim().ToUpper();
            if (!string.IsNullOrEmpty(number))
            {
                OrderTracesData RequestList = null;
                //快递单号识别
                ExpressKDNDTO expressdata = OrderTracesSubByJson(number);
                if (expressdata.Success)
                {
                    if (!expressdata.Shippers.IsNullOrEmpty())
                    {
                        if (expressdata.Shippers.Any())
                        {
                            string shipperCode = expressdata.Shippers[0].ShipperCode;
                            if (shipperCode.Contains("SF"))
                            {
                                RequestList = SFQueryOrderTracesSub(number, shipperCode, phone);
                            }
                            else
                            {
                                RequestList = QueryOrderTracesSub(number, shipperCode);
                            }

                            RequestList.ShipperName = expressdata.Shippers[0].ShipperName;
                            if (RequestList.Success)
                            {
                                if (RequestList.Traces != null)
                                {
                                    RequestList.Traces = RequestList.Traces.OrderByDescending(p => p.AcceptTime).ToList();
                                }
                                return RequestList;
                            }
                            else
                            {
                                return new OrderTracesData
                                {
                                    LogisticCode = number,
                                    State = 500,
                                    Success = false,
                                    Traces = null,
                                    Reason = RequestList.Reason + ";RequestList:" + RequestList.ToJson(),
                                };
                            }
                        }
                        else
                        {
                            return new OrderTracesData
                            {
                                LogisticCode = number,
                                State = 400,
                                Success = false,
                                Traces = null,
                                Reason = "快递单号识别错误"
                            };
                        }
                    }
                    else
                    {
                        return new OrderTracesData
                        {
                            LogisticCode = number,
                            State = 400,
                            Success = false,
                            Traces = null,
                            Reason = "快递单号识别错误"
                        };
                    }
                }
                else
                {
                    return new OrderTracesData
                    {
                        LogisticCode = number,
                        State = 400,
                        Success = false,
                        Traces = null,
                        Reason = "快递单号识别错误;" + "expressdata:" + expressdata.ToJson()
                    };
                }
            }
            return new OrderTracesData
            {
                LogisticCode = number,
                State = 500,
                Success = false,
                Traces = null,
                Reason = "暂未查询到该快递单号的消息，请核对单号后重试",
            };
        }

        /// <summary>
        ///  物流信息订阅
        /// </summary>
        /// <returns></returns>
        public static OrderTracesData QueryOrderTracesSub(string number, string shipperCode)
        {
            string requestData = "{'OrderCode':'','ShipperCode':'" + shipperCode + "','LogisticCode':'" + number + "'}";

            Dictionary<string, object> param = new Dictionary<string, object>
            {
                { "RequestData", HttpUtility.UrlEncode(requestData, Encoding.UTF8) },
                { "EBusinessID", EBusinessID },
                { "RequestType", "8001" }
            };
            string dataSign = encrypt(requestData, AppKey, "UTF-8");
            param.Add("DataSign", HttpUtility.UrlEncode(dataSign, Encoding.UTF8));
            param.Add("DataType", "2");

            string data = HttpHelper.PostData(ReqURL, param);

            OrderTracesData result = JsonConvert.DeserializeObject<OrderTracesData>(data);
            //根据公司业务处理返回的信息......

            return result;
        }

        /// <summary>
        /// 顺丰物流信息订阅
        /// </summary>
        /// <returns></returns>
        public static OrderTracesData SFQueryOrderTracesSub(string number, string shipperCode, string phone)
        {
            //{'LogisticCode':'233823364856','ShipperCode':'SF','CustomerName':'1234'}
            string requestData = "{'OrderCode':'','ShipperCode':'" + shipperCode + "','LogisticCode':'" + number + "','CustomerName':'" + phone + "'}";

            Dictionary<string, object> param = new Dictionary<string, object>
            {
                { "RequestData", HttpUtility.UrlEncode(requestData, Encoding.UTF8) },
                { "EBusinessID", EBusinessID },
                { "RequestType", "8001" }
            };
            string dataSign = encrypt(requestData, AppKey, "UTF-8");
            param.Add("DataSign", HttpUtility.UrlEncode(dataSign, Encoding.UTF8));
            param.Add("DataType", "2");

            string data = HttpHelper.PostData(ReqURL, param);

            OrderTracesData result = JsonConvert.DeserializeObject<OrderTracesData>(data);
            //根据公司业务处理返回的信息......

            return result;
        }


        /// <summary>
        /// 快递单好识别
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static async Task<ExpressKDNDTO> OrderTracesSub(string number)
        {
            ExpressKDNDTO data = null;
            await Task.Run(() =>
            {
                data = OrderTracesSubByJson(number);
            });
            return data;
        }

        /// <summary>
        /// 单号识别
        /// </summary>
        /// <returns></returns>
        public static ExpressKDNDTO OrderTracesSubByJson(string number)
        {
            string requestData = "{'LogisticCode': '" + number + "'}";

            Dictionary<string, object> param = new Dictionary<string, object>
            {
                { "RequestData", HttpUtility.UrlEncode(requestData, Encoding.UTF8) },
                { "EBusinessID", EBusinessID },
                { "RequestType", "2002" }
            };
            string dataSign = encrypt(requestData, AppKey, "UTF-8");
            param.Add("DataSign", HttpUtility.UrlEncode(dataSign, Encoding.UTF8));
            param.Add("DataType", "2");

            string data = HttpHelper.PostData(ReqURL, param);

            ExpressKDNDTO result = JsonConvert.DeserializeObject<ExpressKDNDTO>(data);
            //根据公司业务处理返回的信息......

            return result;
        }

        ///<summary>
        ///电商Sign签名
        ///</summary>
        ///<param name="content">内容</param>
        ///<param name="keyValue">Appkey</param>
        ///<param name="charset">URL编码 </param>
        ///<returns>DataSign签名</returns>
        private static string encrypt(string content, string keyValue, string charset)
        {
            if (keyValue != null)
            {
                string key = content + keyValue;
                return base64(key.ToMD5String(), charset);
            }
            else
            {
                return base64(content.ToMD5String(), charset);
            }
        }

        #endregion
        /// <summary>
        /// base64编码
        /// </summary>
        /// <param name="str">内容</param>
        /// <param name="charset">编码方式</param>
        /// <returns></returns>
        private static string base64(string str, string charset)
        {
            return Convert.ToBase64String(System.Text.Encoding.GetEncoding(charset).GetBytes(str));
        }
    }
}
