using Newtonsoft.Json.Linq;
using Shipeng.Util.Helper;

namespace Shipeng.Util.API
{
    /// <summary>
    /// (新)鼎薪宝付款系统api 接口文档：https://www.showdoc.com.cn/dxbD/7347793712165947
    /// </summary>
    public static class DingShuimAoHelper
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
                { "appId", GlobalData.XinDingXinBaoAppId },
                { "appNo", GlobalData.XinDingXinBaoAppKey }
            };
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.XinDingXinBaoSystemKey }
            };
            string ret = await HttpClientHelper.GetAsync(GlobalData.XinDingXinBaoUrl + "openApi/third/newestAuthToken", data, header);
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
        /// 提现结算申请(单数据)
        /// </summary>
        /// <returns> </returns>
        public static async Task<string> WithdrawSettleApplyAsync(WithdrawSettleApply list, string Token = "")
        {
            if (list.IsNullOrEmpty())
            {
                throw new BusException("数据不能为空...");
            }

            if (Token.IsNullOrEmpty())
            {
                Token = await GetTokenAsync();
            }
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.XinDingXinBaoSystemKey },
                { "token",Token }
            };
            string html = list.ToJson();
            html = AesHelper.AESEncrypts(html, GlobalData.XinDingXinBaoAesKey);

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "encrypt",html}
            };
            string ret = await HttpClientHelper.PostAsync(GlobalData.XinDingXinBaoUrl + "openApi/settle/withdraw/settle/Apply", data, header);
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
        /// 项目结算申请(多条)
        /// </summary>
        /// <returns> </returns>
        public static async Task<string> SettleSettleProjectAsync(SettleSettleProject list, string Token = "")
        {
            if (list.IsNullOrEmpty())
            {
                throw new BusException("数据不能为空...");
            }

            if (Token.IsNullOrEmpty())
            {
                Token = await GetTokenAsync();
            }
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.XinDingXinBaoSystemKey },
                { "token",Token }
            };
            string html = list.ToJson();
            html = AesHelper.AESEncrypts(html, GlobalData.XinDingXinBaoAesKey);

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "encrypt",html}
            };
            string ret = await HttpClientHelper.PostAsync(GlobalData.XinDingXinBaoUrl + "openApi/settle/settleProject", data, header);
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
        /// <param name="settleType">结算类型（2项目结算；3提现结算）</param>
        /// <param name="settleNo">申请流水号或项目编码</param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static async Task<string> GetApplyOrder(string settleType, string settleNo, string Token = "")
        {
            if (Token.IsNullOrEmpty())
            {
                Token = await GetTokenAsync();
            }
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.XinDingXinBaoSystemKey },
                { "token",Token }
            };

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                {"settleType",settleType },
                { "settleNo",settleNo}
            };
            string ret = await HttpClientHelper.PostAsync(GlobalData.XinDingXinBaoUrl + "openApi/settle/getApplyOrder", data, header);
            JObject jo = JObject.Parse(ret);
            //string keydata = AesHelper.AESDecrypt(jo["data"].ToString(), GlobalData.XinDingXinBaoAesKey);//解密
            jo["data"] = EncryptUtils.decrypt(jo["data"].ToString(), GlobalData.XinDingXinBaoAesKey);//解密
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


        /// <summary>
        /// 账户余额查询
        /// </summary>
        /// <param name="name">用工企业名称</param>
        /// <param name="code">用工企业统一社会信用代码</param>
        /// <param name="Token"></param>
        /// <returns>返回数据为加密数据</returns>
        public static async Task<string> GetBalance(string name, string code, string Token = "")
        {
            if (Token.IsNullOrEmpty())
            {
                Token = await GetTokenAsync();
            }
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.XinDingXinBaoSystemKey },
                { "token",Token }
            };
            Dictionary<string, string> data = new Dictionary<string, string>
            {{"name",name },
                { "code",code}
            };
            string ret = await HttpClientHelper.PostAsync(GlobalData.XinDingXinBaoUrl + "openApi/account/getBalance", data, header);
            //ret = AesHelper.AESDecrypt(ret, GlobalData.XinDingXinBaoAesKey);//解密
            JObject jo = JObject.Parse(ret);
            jo["data"] = EncryptUtils.decrypt(jo["data"].ToString(), GlobalData.XinDingXinBaoAesKey);//解密
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

        /// <summary>
        /// 查询提现申请回单
        /// </summary>
        /// <param name="applyNo">申请流水号</param>
        /// <param name="Token"></param>
        /// <returns>返回数据为加密数据</returns>
        public static async Task<string> GetApplyReceipt(string applyNo, string Token = "")
        {
            if (Token.IsNullOrEmpty())
            {
                Token = await GetTokenAsync();
            }
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.XinDingXinBaoSystemKey },
                { "token",Token }
            };

            Dictionary<string, string> data = new Dictionary<string, string>
            {{"applyNo",applyNo }};
            string ret = await HttpClientHelper.PostAsync(GlobalData.XinDingXinBaoUrl + "openApi/settle/getApplyReceipt", data, header);
            //ret = AesHelper.AESDecrypt(ret, GlobalData.XinDingXinBaoAesKey);//解密
            JObject jo = JObject.Parse(ret);
            jo["data"] = EncryptUtils.decrypt(jo["data"].ToString(), GlobalData.XinDingXinBaoAesKey);//解密
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

        /// <summary>
        /// 查询项目请回单
        /// </summary>
        /// <param name="projectCode">项目编码</param>
        /// <param name="Token"></param>
        /// <returns>返回数据为加密数据</returns>
        public static async Task<string> GetProjectReceipt(string projectCode, string Token = "")
        {
            if (Token.IsNullOrEmpty())
            {
                Token = await GetTokenAsync();
            }
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "accessKey",GlobalData.XinDingXinBaoSystemKey },
                { "token",Token }
            };

            Dictionary<string, string> data = new Dictionary<string, string>
            {{"projectCode",projectCode }
            };
            string ret = await HttpClientHelper.PostAsync(GlobalData.XinDingXinBaoUrl + "openApi/settle/getProjectReceipt", data, header);

            //ret = AesHelper.AESDecrypt(ret, GlobalData.XinDingXinBaoAesKey);//解密
            JObject jo = JObject.Parse(ret);
            jo["data"] = EncryptUtils.decrypt(jo["data"].ToString(), GlobalData.XinDingXinBaoAesKey);//解密
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

        ////结算回调通知 需要自己写
    }

    /// <summary>
    /// 付款实体  单
    /// </summary>
    public class WithdrawSettleApply
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
        /// 收款方式(1银行卡;2支付宝;3微信)
        /// </summary>
        public string payType { get; set; }

        /// <summary>
        /// 	收款账号（银行卡号，支付宝账号、微信收款openId）
        /// </summary>
        public string payAccount { get; set; }

        /// <summary>
        /// 收款金额（大于零，最多两位小数）
        /// </summary>
        public string payAmount { get; set; }

        /// <summary>
        /// 申请流水号（调入方唯一识别号）
        /// </summary>
        public string applyNo { get; set; }

        /// <summary>
        /// 申请描述 提现平台名称和平台账号;示例：抖音，jsdingshun）
        /// </summary>
        public string applyContent { get; set; }

        /// <summary>
        /// 所属行业，请参考(https://www.showdoc.com.cn/dxbD/7347794628541383)
        /// </summary>
        public string industry { get; set; }

        /// <summary>
        /// 结算企业名称
        /// </summary>
        public string enterpriseName { get; set; }

        /// <summary>
        /// 结算企业社会信用代码
        /// </summary>
        public string code { get; set; }
    }

    /// <summary>
    /// 付款实体 多
    /// </summary>
    public class SettleSettleProject
    {
        /// <summary>
        /// 企业名称
        /// </summary>
        public string name { get; set; } //是   string 企业名称

        /// <summary>
        /// 统一社会信用代码
        /// </summary>
        public string code { get; set; } //是   string 统一社会信用代码

        /// <summary>
        /// 项目名称
        /// </summary>
        public string projectName { get; set; }//是   string 项目名称

        /// <summary>
        /// 项目编码；接入方唯一识别码
        /// </summary>
        public string projectCode { get; set; } //是   string 项目编码；接入方唯一识别码

        /// <summary>
        /// 所属行业，请参考《行业字典说明》
        /// </summary>
        public string industry { get; set; }   //是 string 所属行业，请参考《行业字典说明》

        /// <summary>
        /// 项目内容描述
        /// </summary>
        public string content { get; set; } //是  // string 项目内容描述

        /// <summary>
        /// 岗位名称
        /// </summary>
        public string job { get; set; } //是   //string 岗位名称

        /// <summary>
        /// 岗位内容描述
        /// </summary>
        public string jobContent { get; set; }//是   string 岗位内容描述

        /// <summary>
        /// 工作联系方式
        /// </summary>
        public string jobPhone { get; set; }//是   string 工作联系方式

        /// <summary>
        /// 工作地址
        /// </summary>
        public string jobAddress { get; set; }//是   string 工作地址

        /// <summary>
        /// 结算人数
        /// </summary>
        public string settleNumber { get; set; }// 是   string 结算人数

        /// <summary>
        /// 佣金计算规则描述
        /// </summary>
        public string settle { get; set; }//是   string 佣金计算规则描述

        /// <summary>
        /// 项目结算金额
        /// </summary>
        public string settleMoney { get; set; }// 是   string 项目结算金额

        /// <summary>
        /// 结算人员数据集合
        /// </summary>
        public List<SettleSettleProjectUser> users { get; set; }// 是   list 结算人员数据集合
    }

    /// <summary>
    /// 具体用户数据
    /// </summary>
    public class SettleSettleProjectUser
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string name { get; set; }//  是   string 姓名

        /// <summary>
        /// 手机号
        /// </summary>
        public string phone { get; set; }//  是   string 手机号

        /// <summary>
        /// 身份证号
        /// </summary>
        public string cardNo { get; set; }//  是   string 身份证号

        /// <summary>
        /// 收款方式(1银行卡;2支付宝;3微信)
        /// </summary>
        public string payType { get; set; }//  是   string 收款方式(1银行卡;2支付宝;3微信)

        /// <summary>
        /// 收款账号（银行卡号，支付宝账号、微信收款openId）
        /// </summary>
        public string payAccount { get; set; }//  是   string 收款账号（银行卡号，支付宝账号、微信收款openId）

        /// <summary>
        /// 收款金额
        /// </summary>
        public string payAmount { get; set; }//  是   string 收款金额
    }

    /// <summary>
    /// 回调接口返回实体
    /// </summary>
    public class XinDingXinBaoSett
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

    public class XinDingXinBaoDetAppOrde
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

    public class DingXinBaoSettleApplyOrder
    {
        /// <summary>
        /// 结算类型（1提现结算；2项目结算）
        /// </summary>
        public string settleType { get; set; }

        /// <summary>
        /// 申请流水号 或 项目编码
        /// </summary>
        public string settleNo { get; set; }

        public List<SettleApplyOrderUserList> users { get; set; }
    }

    public class SettleApplyOrderUserList
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string name { get; set; }


        /// <summary>
        /// 身份证号
        /// </summary>
        public string cardNo { get; set; }


        /// <summary>
        /// 结算金额
        /// </summary>
        public string settleAmount { get; set; }


        /// <summary>
        /// 1待商户付款/2交易成功/3交易失败/4已退款/5驳回/6待审核
        /// </summary>
        public int status { get; set; }


        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime payTime { get; set; }


        /// <summary>
        /// 失败原因
        /// </summary>
        public string failReason { get; set; }
    }
}