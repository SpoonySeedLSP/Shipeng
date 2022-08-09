using Shipeng.Util.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Shipeng.Util
{
    public class WeChatPayHelper
    {
        public static async Task<(bool result, string msg, WxPayData data)> GetUnifiedOrderResultAsync(JsApiUnifiedorderDto dto)
        {
            return await Task.Run(() =>
            {
                return GetUnifiedOrderResult(dto);
            });
        }

        public static (bool result, string msg, WxPayData data) GetUnifiedOrderResult(JsApiUnifiedorderDto dto)
        {
            //统一下单
            WxPayData data = new WxPayData();
            data.SetValue("body", dto.body);
            //data.SetValue("attach", attachStr);
            data.SetValue("out_trade_no", dto.orderId);
            data.SetValue("total_fee", dto.total_fee);
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            //data.SetValue("goods_tag", "");
            data.SetValue("trade_type", dto.trade_type);
            if (dto.trade_type.Equals("NATIVE"))
            {
                data.SetValue("product_id", dto.product_id);
            }
            else
            {
                data.SetValue("openid", dto.openid);
            }
            data.SetValue("notify_url", dto.notify_url);
            data.SetValue("appid", dto.appid);//公众账号ID
            data.SetValue("mch_id", dto.mch_id);//商户号
            WxPayData result = UnifiedOrder(data);
            if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
            {
                string msg = result.ToJson();
                /*if (!result.GetValue("err_code").IsNullOrEmpty())
                {
                    if (result.GetValue("err_code").ToString() == "FAIL")
                    {
                        msg = result.GetValue("err_code_des").ToString();
                    }
                    else
                    {
                        msg = "请稍后再试！";
                    }
                }

                if (!result.GetValue("return_code").IsNullOrEmpty())
                {
                    if (result.GetValue("return_code").ToString() != "SUCCESS")
                    {
                        msg = result.GetValue("return_msg").ToString();
                    }
                    else
                    {
                        msg = "请稍后再试！";
                    }
                }*/

                return (false, msg, null);
            }

            unifiedOrderResult = result;
            return (true, "success", result);
        }

        private static WxPayData UnifiedOrder(WxPayData inputObj)
        {
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no"))
            {
                throw new Exception("缺少统一支付接口必填参数out_trade_no！");
            }
            else if (!inputObj.IsSet("body"))
            {
                throw new Exception("缺少统一支付接口必填参数body！");
            }
            else if (!inputObj.IsSet("total_fee"))
            {
                throw new Exception("缺少统一支付接口必填参数total_fee！");
            }
            else if (!inputObj.IsSet("trade_type"))
            {
                throw new Exception("缺少统一支付接口必填参数trade_type！");
            }

            //关联参数
            if (inputObj.GetValue("trade_type").ToString() == "JSAPI" && !inputObj.IsSet("openid"))
            {
                throw new Exception("统一支付接口中，缺少必填参数openid！trade_type为JSAPI时，openid为必填参数！");
            }
            if (inputObj.GetValue("trade_type").ToString() == "NATIVE" && !inputObj.IsSet("product_id"))
            {
                throw new Exception("统一支付接口中，缺少必填参数product_id！trade_type为JSAPI时，product_id为必填参数！");
            }

            inputObj.SetValue("spbill_create_ip", "8.8.8.8");//终端ip
            inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串
            inputObj.SetValue("sign_type", WxPayData.SIGN_TYPE_MD5);//签名类型

            //签名
            inputObj.SetValue("sign", inputObj.MakeSign());
            string xml = inputObj.ToXml();

            DateTime start = DateTime.Now;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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

            DateTime end = DateTime.Now;
            int timeCost = (int)((end - start).TotalMilliseconds);

            WxPayData result = new WxPayData();
            result.FromXml(resData);
            //ReportCostTime(url, timeCost, result);//测速上报

            return result;
        }

        public static async Task<string> GetJsApiParametersAsync()
        {
            return await Task.Run(() =>
            {
                return GetJsApiParameters();
            });
        }

        public static string GetJsApiParameters()
        {
            WxPayData jsApiParam = new WxPayData();
            jsApiParam.SetValue("appId", unifiedOrderResult.GetValue("appid"));
            jsApiParam.SetValue("timeStamp", GenerateTimeStamp());
            jsApiParam.SetValue("nonceStr", GenerateNonceStr());
            jsApiParam.SetValue("package", "prepay_id=" + unifiedOrderResult.GetValue("prepay_id"));
            jsApiParam.SetValue("signType", "MD5");
            jsApiParam.SetValue("paySign", jsApiParam.MakeSign());

            string parameters = jsApiParam.ToJson();
            return parameters;
        }

        /// <summary>
        /// 根据当前系统时间加随机序列来生成订单号
        /// </summary>
        /// <returns> </returns>
        private static string GenerateOutTradeNo()
        {
            Random ran = new Random();
            return string.Format("{0}{1}{2}", "1582768421", DateTime.Now.ToString("yyyyMMddHHmmss"), ran.Next(999));
        }

        /// <summary>
        /// 统一下单接口返回结果
        /// </summary>
        private static WxPayData unifiedOrderResult { get; set; }

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

        public static async Task<(bool success, string msg, string result)> ApplyRefundAsync(ApplyRefund dto)
        {
            return await Task.Run(() =>
            {
                return ApplyRefund(dto);
            });
        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="dto"> </param>
        /// <returns> </returns>
        public static (bool success, string msg, string result) ApplyRefund(ApplyRefund dto)
        {
            if (!File.Exists(dto.certurl))
            {
                return (false, dto.ToJson(), "微信支付证书不存在");
            }
            WxPayData data = new WxPayData();
            if (!string.IsNullOrEmpty(dto.transaction_id))//微信订单号存在的条件下，则已微信订单号为准
            {
                data.SetValue("transaction_id", dto.transaction_id);
            }
            else//微信订单号不存在，才根据商户订单号去退款
            {
                data.SetValue("out_trade_no", dto.out_trade_no);
            }

            data.SetValue("total_fee", dto.total_fee);//订单总金额
            data.SetValue("refund_fee", dto.refund_fee);//退款金额
            data.SetValue("out_refund_no", dto.out_refund_no);//随机生成商户退款单号
            if (!dto.refund_desc.IsNullOrEmpty())
            {
                data.SetValue("refund_desc", dto.refund_desc);
            }
            //data.SetValue("op_user_id", dto.mch_id);//操作员，默认为商户号
            data.SetValue("notify_url", dto.notify_url);//操作员，默认为商户号
            data.SetValue("appid", dto.appid);//公众账号ID
            data.SetValue("mch_id", dto.mch_id);//商户号
            WxPayData result = Refund(data, dto.certurl, dto.mch_id);//提交退款申请给API，接收返回数据
            //LogHelper.WriteLog_WeChartTxtB("微信退款返回2:" + data.ToJson());
            if (result.GetValue("return_code").ToString() == "SUCCESS")
            {
                if (result.GetValue("result_code").ToString() == "SUCCESS")
                {
                    return (true, "", result.ToPrintStr());
                }
                else
                {
                    return (false, result.ToJson(), result.ToPrintStr());
                }
            }
            else
            {
                return (false, result.ToJson(), result.ToPrintStr());
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="inputObj"> </param>
        /// <param name="certurl"> </param>
        /// <param name="MchId"></param>
        /// <returns> </returns>
        private static WxPayData Refund(WxPayData inputObj, string certurl, string MchId)
        {
            string url = "https://api.mch.weixin.qq.com/secapi/pay/refund";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no") && !inputObj.IsSet("transaction_id"))
            {
                throw new Exception("退款申请接口中，out_trade_no、transaction_id至少填一个！");
            }
            else if (!inputObj.IsSet("out_refund_no"))
            {
                throw new Exception("退款申请接口中，缺少必填参数out_refund_no！");
            }
            else if (!inputObj.IsSet("total_fee"))
            {
                throw new Exception("退款申请接口中，缺少必填参数total_fee！");
            }
            else if (!inputObj.IsSet("refund_fee"))
            {
                throw new Exception("退款申请接口中，缺少必填参数refund_fee！");
            }
            //else if (!inputObj.IsSet("op_user_id"))
            //{
            //    throw new Exception("退款申请接口中，缺少必填参数op_user_id！");
            //}
            inputObj.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));//随机字符串
            inputObj.SetValue("sign_type", WxPayData.SIGN_TYPE_MD5);//签名类型
            inputObj.SetValue("sign", inputObj.MakeSign());//签名

            string xml = inputObj.ToXml();
            DateTime start = DateTime.Now;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            X509Certificate2 cert = new X509Certificate2(certurl, MchId, X509KeyStorageFlags.MachineKeySet);
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

            DateTime end = DateTime.Now;
            //int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时

            //将xml格式的结果转换为对象以返回
            WxPayData result = new WxPayData();
            LogHelper.WriteLog_WeChartTxtB("微信退款返回1:" + result.ToJson());
            result.FromXml(resData);

            //ReportCostTime(url, timeCost, result);//测速上报

            return result;
        }

        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <param name="context"> </param>
        /// <returns> </returns>
        public static (bool result, ProcessNotifyReturn data) ProcessNotify(HttpContext context)
        {
            WxPayData notifyData = GetNotifyData(context);

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                ////若transaction_id不存在，则立即返回结果给微信支付后台
                //WxPayData res = new WxPayData();
                //res.SetValue("return_code", "FAIL");
                //res.SetValue("return_msg", "支付结果中微信订单号不存在");
                //context.Response.WriteAsync(res.ToXml());
                return (false, null);
            }
            string AppId = "";
            string MchId = "";
            if (!notifyData.IsSet("appid"))
            {
                AppId = notifyData.GetValue("appid").ToString();
            }

            if (!notifyData.IsSet("mch_id"))
            {
                MchId = notifyData.GetValue("mch_id").ToString();
            }

            if (AppId.IsNullOrEmpty())
            {
                AppId = GlobalData.AppletId;
            }
            if (MchId.IsNullOrEmpty())
            {
                MchId = GlobalData.MchId;
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();
            string out_trade_no = notifyData.GetValue("out_trade_no").ToString();

            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id, MchId, AppId))
            {
                ////若订单查询失败，则立即返回结果给微信支付后台
                //WxPayData res = new WxPayData();
                //res.SetValue("return_code", "FAIL");
                //res.SetValue("return_msg", "订单查询失败");
                //context.Response.WriteAsync(res.ToXml());
                return (false, null);
            }
            //查询订单成功
            else
            {
                //WxPayData res = new WxPayData();
                //res.SetValue("return_code", "SUCCESS");
                //res.SetValue("return_msg", "OK");
                //context.Response.WriteAsync(res.ToXml());
                ProcessNotifyReturn notifyReturn = new ProcessNotifyReturn
                {
                    out_trade_no = out_trade_no,
                    transaction_id = transaction_id
                };
                return (true, notifyReturn);
            }
        }

        /// <summary>
        /// 接收从微信支付后台发送过来的数据并验证签名
        /// </summary>
        /// <returns> 微信支付后台返回的数据 </returns>
        public static WxPayData GetNotifyData(HttpContext context)
        {
            #region

            //注意：如果用以下读取流的方法，.net core 3.0 以后一定要加下边那段
            //.net core 3.0以后需加下边这段，否则Stream会报错
            IHttpBodyControlFeature syncIOFeature = context.Features.Get<IHttpBodyControlFeature>();
            if (syncIOFeature != null)
            {
                syncIOFeature.AllowSynchronousIO = true;
            }

            #endregion

            //接收从微信后台POST过来的数据
            Stream s = context.Request.Body;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            //s.Flush();
            s.Close();
            s.Dispose();

            //转换数据格式并验证签名
            WxPayData data = new WxPayData();
            try
            {
                data.FromXml(builder.ToString());
            }
            catch (Exception ex)
            {
                ////若签名错误，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                context.Response.WriteAsync(res.ToXml());
            }
            return data;
        }

        /// <summary>
        /// </summary>
        /// <param name="transaction_id"> </param>
        /// <param name="MchId"></param>
        /// <param name="AppId"></param>
        /// <returns> </returns>
        private static bool QueryOrder(string transaction_id, string MchId, string AppId)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = OrderQuery(req, MchId, AppId);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="inputObj"> </param>
        /// <param name="MchId"></param>
        /// <param name="AppId"></param>
        /// <returns> </returns>
        public static WxPayData OrderQuery(WxPayData inputObj, string MchId, string AppId)
        {
            //string appid = "wxc8e8600777c78ceb";
            //string mchid = "1610103915";
            string url = "https://api.mch.weixin.qq.com/pay/orderquery";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no") && !inputObj.IsSet("transaction_id"))
            {
                throw new Exception("订单查询接口中，out_trade_no、transaction_id至少填一个！");
            }

            inputObj.SetValue("appid", AppId);//公众账号ID
            inputObj.SetValue("mch_id", MchId);//商户号
            inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串
            inputObj.SetValue("sign_type", "MD5");//签名类型
            inputObj.SetValue("sign", inputObj.MakeSign());//签名

            string xml = inputObj.ToXml();
            DateTime start = DateTime.Now;
            //string response = HttpHelper.Post(xml, url, false, timeOut);//调用HTTP通信接口提交数据
            string response = HttpHelper.PostHttpResponse(url, xml);//调用HTTP通信接口提交数据
            DateTime end = DateTime.Now;
            int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时

            //将xml格式的数据转化为对象以返回
            WxPayData result = new WxPayData();
            result.FromXml(response);

            //ReportCostTime(url, timeCost, result);//测速上报
            return result;
        }

        public static string RefundCallBack(HttpContext context)
        {
            string out_refund_no = "";

            #region

            //.net core 3.0以后需加下边这段，否则Stream会报错
            IHttpBodyControlFeature syncIOFeature = context.Features.Get<IHttpBodyControlFeature>();
            if (syncIOFeature != null)
            {
                syncIOFeature.AllowSynchronousIO = true;
            }

            #endregion

            //接收从微信后台POST过来的数据
            System.IO.Stream s = context.Request.Body;
            int count = 0;
            byte[] buffer = new byte[10 * 1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            //s.Flush();
            s.Close();
            s.Dispose();

            //转换数据格式并验证签名
            WxPayData notifyData = new WxPayData();
            notifyData.NoSignFromXml(builder.ToString());

            if (notifyData.GetValue("return_code").ToString() == "SUCCESS")
            {
                //WxPayData res = new WxPayData();
                //res.SetValue("return_code", "SUCCESS");
                //res.SetValue("return_msg", "OK");
                //context.Response.WriteAsync(res.ToXml());

                string req_info = notifyData.GetValue("req_info").ToString();
                string key = "bb625bbab068c72c7a3e681029b38dc5";     //xxx是微信商户key
                key = key.ToMD5String().ToLower();
                req_info = WxPayData.DecodeAES256ECB(req_info, key);

                WxPayData data = new WxPayData();
                data.NoSignFromXml(req_info);

                if (data.GetValue("refund_status").ToString() == "SUCCESS")
                {
                    out_refund_no = data.GetValue("out_refund_no").ToString();
                }
            }
            return out_refund_no;
        }
    }
}

public class JsApiUnifiedorderDto
{
    public string body { get; set; }
    public string orderId { get; set; }
    public int total_fee { get; set; }
    public string notify_url { get; set; }
    public string openid { get; set; }
    public string appid { get; set; }
    public string key { get; set; }
    public string mch_id { get; set; }
    public string product_id { get; set; }
    public string trade_type { get; set; }
}

public class ApplyRefund
{
    /// <summary>
    /// 微信订单号（和out_trade_no二传一，优先级transaction_id&gt;out_trade_no）
    /// </summary>
    public string transaction_id { get; set; }

    /// <summary>
    /// 商户订单号（和transaction_id二传一，优先级transaction_id&gt;out_trade_no）
    /// </summary>
    public string out_trade_no { get; set; }

    /// <summary>
    /// 商户系统内部的退款单号
    /// </summary>
    public string out_refund_no { get; set; }

    /// <summary>
    /// 订单总金额（分）
    /// </summary>
    public int total_fee { get; set; }

    /// <summary>
    /// 退款金额（分）
    /// </summary>
    public int refund_fee { get; set; }

    /// <summary>
    /// 退款回调地址
    /// </summary>
    public string notify_url { get; set; }

    public string appid { get; set; }
    /// <summary>
    /// 小程序key
    /// </summary>
    public string key { get; set; }
    public string mch_id { get; set; }
    public string refund_desc { get; set; }

    /// <summary>
    /// 证书路径
    /// </summary>
    public string certurl { get; set; }
}

public class ProcessNotifyReturn
{
    public string out_trade_no { get; set; }
    public string transaction_id { get; set; }
}
