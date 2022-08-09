using Shipeng.Util.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shipeng.Util.API
{
    /// <summary>
    /// 鼎薪宝付款系统api 接口文档：https://www.showdoc.com.cn/dxbO/5611626519426017
    /// </summary>
    public static class DingXinBaoHelper
    {
        /// <summary>
        /// 获取Token
        /// </summary>
        /// <returns> </returns>
        public static async Task<string> GetTokenAsync()
        {
            string Token = "";
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "appId", GlobalData.DingXinBaoAppId },
                { "appNo", GlobalData.DingXinBaoAppKey }
            };
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.DingXinBaoSystemKey }
            };
            string ret = await HttpClientHelper.GetAsync(GlobalData.DingXinBaoUrl + "openApi/third/authToken", data, header);
            JObject jo = JObject.Parse(ret);

            if (jo["code"].ToString() == "200")
            {
                Token = jo["data"]["token"].ToString();
            }
            else
            {
                throw new BusException("请求错误code:" + jo["code"].ToString() + ",错误原因:" + jo["message"].ToString());
            }
            return Token;
        }

        /// <summary>
        /// 付款
        /// </summary>
        /// <returns> </returns>
        public static async Task<string> SetsettleApplyAsync(List<DingXinBaoSettleApply> list, string Token = "")
        {
            if (list.Count <= 0)
            {
                throw new BusException("数据不能为空...");
            }
            list.ForEach(aDataSource =>
            {
                aDataSource.custNo = GlobalData.DingXinBaoAppKey;
            });
            if (Token.IsNullOrEmpty())
            {
                Token = await GetTokenAsync();
            }
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.DingXinBaoSystemKey },
                { "token",Token }
            };
            string html = "{\"list\":" + list.ToJson() + "}";
            html = AesHelper.AESEncrypts(html, GlobalData.DingXinBaoAesKey);

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "encrypt",html}
            };
            string ret = await HttpClientHelper.PostAsync(GlobalData.DingXinBaoUrl + "openApi/settle/settleApply", data, header);
            JObject jo = JObject.Parse(ret);
            if (jo["code"].ToString() == "200")
            {
                LogHelper.WriteLog_DingxinbaoTxt(jo["code"].ToString());
                ret = jo.ToString();
            }
            else
            {
                LogHelper.WriteLog_DingxinbaoTxt(jo["code"].ToString());
                throw new BusException("请求错误code:" + jo["code"].ToString() + ",错误原因:" + jo["message"].ToString());
            }
            return ret;
        }

        /// <summary>
        /// 查询处理结果
        /// </summary>
        /// <param name="applyNo"> </param>
        /// <param name="Token"> </param>
        /// <returns> </returns>
        public static async Task<string> GetApplyOrder(string applyNo, string Token = "")
        {
            if (Token.IsNullOrEmpty())
            {
                Token = await GetTokenAsync();
            }
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.DingXinBaoSystemKey },
                { "token",Token }
            };

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "applyNo",applyNo}
            };
            string ret = await HttpClientHelper.PostAsync(GlobalData.DingXinBaoUrl + "openApi/settle/getApplyOrder", data, header);
            JObject jo = JObject.Parse(ret);
            if (jo["code"].ToString() == "200")
            {
                LogHelper.WriteLog_DingxinbaoTxt(jo["code"].ToString());
                ret = jo.ToString();
            }
            else
            {
                LogHelper.WriteLog_DingxinbaoTxt(jo["code"].ToString());
                //throw new BusException("请求错误code:" + jo["code"].ToString() + ",错误原因:" + jo["message"].ToString());
                ret = jo.ToString();
            }
            return ret;
        }
    }

    /// <summary>
    /// 付款实体
    /// </summary>
    public class DingXinBaoSettleApply
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string cardNo { get; set; }

        /// <summary>
        /// 身份证正面地址:url路径，仅支持jpg,png格式图片
        /// </summary>
        public string cardFront { get; set; }

        /// <summary>
        /// 身份证反面地址:url路径，仅支持jpg,png格式图片
        /// </summary>
        public string cardBack { get; set; }

        /// <summary>
        /// 收款方式(1银行卡;2支付宝;4电子账户;6云钱包)
        /// </summary>
        public string payType { get; set; }

        /// <summary>
        /// 收款账号
        /// </summary>
        public string payAccount { get; set; }

        /// <summary>
        /// 收款金额（大于零，最多两位小数）
        /// </summary>
        public string payAmount { get; set; }

        /// <summary>
        /// 应用公钥
        /// </summary>
        public string custNo { get; set; }

        /// <summary>
        /// 申请流水号（调入方唯一识别号）
        /// </summary>
        public string applyNo { get; set; }

        /// <summary>
        /// 申请描述
        /// </summary>
        public string applyContent { get; set; }
    }

    /// <summary>
    /// 回调接口返回实体
    /// </summary>
    public class DingXinBaoSettle
    {
        /// <summary>
        /// 申请流水号
        /// </summary>
        public string applyNo { get; set; }

        /// <summary>
        /// 收款金额
        /// </summary>
        public string payAmount { get; set; }

        /// <summary>
        /// 1待商户付款/2交易成功/3交易失败/4交易取消 0:id不存在
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? payTime { get; set; }
    }

    public class DingXinBaoDetApplyOrder
    {
        /// <summary>
        /// 0:不存在此订单/1待商户付款/2交易成功/3交易失败/4已退款
        /// </summary>
        public int? status { get; set; } = 0;

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime? modifyTime { get; set; }
    }
}
