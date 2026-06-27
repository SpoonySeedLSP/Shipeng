using Shipeng.Dependency;
using Shipeng.JsonSerialization;
using Senparc.Weixin.Work.AdvancedAPIs;
using Senparc.Weixin.Work.AdvancedAPIs.MailList.Member;
using Senparc.Weixin.Work.Containers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shipeng.FriendlyException;

namespace Shipeng.Expand.Thirdparty
{
    /// <summary>
    /// 微信工具类。
    /// 说明：
    /// 1. 企业微信：部门、用户、消息，继续使用 Senparc.Weixin.Work。
    /// 2. 微信小程序：登录、access_token、手机号、订阅消息、数据解密，使用微信官方 HTTP 接口。
    /// 3. 小程序 code2Session 接口没有停用；如果返回 40029，通常是 code 过期、重复使用或 appid/secret 不匹配。
    /// </summary>
    [SuppressSniffer]
    public class WeChat
    {
        /// <summary>
        /// 小程序 HTTP 客户端。
        /// 使用静态 HttpClient，避免每次请求都 new HttpClient 导致端口耗尽。
        /// </summary>
        private static readonly HttpClient MiniProgramHttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        /// <summary>
        /// 企业微信 access_token。
        /// 仅企业微信部门、用户、消息接口使用。
        /// </summary>
        public string accessToken { get; private set; }

        /// <summary>
        /// 小程序 AppID。
        /// 仅小程序接口使用，不能填写企业微信 CorpID。
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 小程序 AppSecret。
        /// 仅小程序接口使用，不能填写企业微信 CorpSecret。
        /// </summary>
        public string secret { get; set; }

        /// <summary>
        /// 企业微信构造方法。
        /// 用于企业微信部门、成员、消息接口。
        /// </summary>
        /// <param name="corpId">企业微信 CorpID。</param>
        /// <param name="corpSecret">企业微信应用 Secret 或通讯录 Secret。</param>
        /// <param name="appSecret">兼容旧调用，如果传了 appSecret，则优先使用 appSecret。</param>
        public WeChat(string corpId, string corpSecret, string appSecret = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(appSecret))
                {
                    corpSecret = appSecret;
                }

                if (string.IsNullOrWhiteSpace(corpId))
                {
                    throw Tracexception.Oh("企业微信 corpId 不能为空");
                }

                if (string.IsNullOrWhiteSpace(corpSecret))
                {
                    throw Tracexception.Oh("企业微信 corpSecret 不能为空");
                }

                accessToken = AccessTokenContainer.TryGetToken(corpId, corpSecret);
            }
            catch (Exception ex)
            {
                throw Tracexception.Oh($"获取企业微信 access_token 失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 微信小程序构造方法。
        /// 用于小程序登录、手机号、订阅消息、数据解密等接口。
        /// </summary>
        /// <param name="appid">小程序 AppID。</param>
        /// <param name="secret">小程序 AppSecret。</param>
        public WeChat(string appid, string secret)
        {
            this.appid = appid;
            this.secret = secret;
        }

        #region 微信小程序授权登录、获取用户信息

        /// <summary>
        /// 小程序登录凭证校验参数。
        /// 前端通过 wx.login 获取 code 后传给后端。
        /// </summary>
        public class Code2SessionParamter
        {
            /// <summary>
            /// 小程序 AppID。
            /// 为空时使用当前 WeChat 实例的 appid。
            /// </summary>
            public string appid { get; set; }

            /// <summary>
            /// 小程序 AppSecret。
            /// 为空时使用当前 WeChat 实例的 secret。
            /// </summary>
            public string secret { get; set; }

            /// <summary>
            /// 前端 wx.login 返回的临时登录凭证。
            /// 注意：只能使用一次，不能缓存，不能重复使用。
            /// </summary>
            public string js_code { get; set; }

            /// <summary>
            /// 授权类型。
            /// 微信官方固定要求填写 authorization_code。
            /// </summary>
            public string grant_type { get; set; } = "authorization_code";

            /// <summary>
            /// 只传 code 的构造方法。
            /// </summary>
            /// <param name="js_code">前端 wx.login 返回的 code。</param>
            public Code2SessionParamter(string js_code)
            {
                this.js_code = js_code;
            }

            /// <summary>
            /// 同时传 appid、secret、code 的构造方法。
            /// </summary>
            /// <param name="appid">小程序 AppID。</param>
            /// <param name="secret">小程序 AppSecret。</param>
            /// <param name="js_code">前端 wx.login 返回的 code。</param>
            public Code2SessionParamter(string appid, string secret, string js_code)
            {
                this.appid = appid;
                this.secret = secret;
                this.js_code = js_code;
            }
        }

        /// <summary>
        /// 小程序 access_token 返回结果。
        /// stable_token 和 token 接口都可以使用该结构。
        /// </summary>
        public class AccessTokenResult
        {
            /// <summary>
            /// 微信返回的 access_token。
            /// </summary>
            public string access_token { get; set; }

            /// <summary>
            /// access_token 有效期，单位秒。
            /// </summary>
            public int expires_in { get; set; }

            /// <summary>
            /// 微信错误码。
            /// 0 表示成功。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息。
            /// </summary>
            public string errmsg { get; set; }
        }

        /// <summary>
        /// 小程序 code2Session 返回结果。
        /// </summary>
        public class Code2SessionResult
        {
            /// <summary>
            /// 用户在当前小程序下的唯一标识。
            /// </summary>
            public string openid { get; set; }

            /// <summary>
            /// 会话密钥。
            /// 用于解密 encryptedData，必须妥善保存，不能下发给前端。
            /// </summary>
            public string session_key { get; set; }

            /// <summary>
            /// 用户在微信开放平台的唯一标识。
            /// 只有小程序绑定开放平台后才可能返回。
            /// </summary>
            public string unionid { get; set; }

            /// <summary>
            /// 微信错误码。
            /// 0 表示成功。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息。
            /// </summary>
            public string errmsg { get; set; }
        }

        /// <summary>
        /// 微信用户性别枚举。
        /// 旧版 encryptedData 用户信息里可能会出现。
        /// </summary>
        public enum Gender
        {
            /// <summary>
            /// 未知。
            /// </summary>
            [Description("未知")]
            unkown = 0,

            /// <summary>
            /// 男。
            /// </summary>
            [Description("男")]
            man = 1,

            /// <summary>
            /// 女。
            /// </summary>
            [Description("女")]
            woman = 2
        }

        /// <summary>
        /// 微信用户信息。
        /// 注意：新版小程序不再建议通过 encryptedData 获取用户昵称头像，建议使用前端能力获取用户授权信息。
        /// </summary>
        public class UserInfo
        {
            /// <summary>
            /// 用户 openId。
            /// </summary>
            public string openId { get; set; }

            /// <summary>
            /// 用户手机号。
            /// </summary>
            public string phoneNumber { get; set; }

            /// <summary>
            /// 用户昵称。
            /// </summary>
            public string nickName { get; set; }

            /// <summary>
            /// 用户性别。
            /// </summary>
            public Gender gender { get; set; }

            /// <summary>
            /// 用户所在国家。
            /// </summary>
            public string country { get; set; }

            /// <summary>
            /// 用户所在省份。
            /// </summary>
            public string province { get; set; }

            /// <summary>
            /// 用户所在城市。
            /// </summary>
            public string city { get; set; }

            /// <summary>
            /// 用户 unionId。
            /// </summary>
            public string unionId { get; set; }

            /// <summary>
            /// 用户头像地址。
            /// </summary>
            public string avatarUrl { get; set; }

            /// <summary>
            /// 微信水印信息。
            /// 可用于校验 appid 和数据时间。
            /// </summary>
            public Watermark watermark { get; set; }
        }

        /// <summary>
        /// 获取手机号接口返回结果。
        /// 官方接口：wxa/business/getuserphonenumber。
        /// </summary>
        public class GetPhoneNumberResult
        {
            /// <summary>
            /// 微信错误码。
            /// 0 表示成功。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息。
            /// </summary>
            public string errmsg { get; set; }

            /// <summary>
            /// 手机号信息。
            /// </summary>
            public PhoneInfo phone_info { get; set; }
        }

        /// <summary>
        /// 微信手机号信息。
        /// </summary>
        public class PhoneInfo
        {
            /// <summary>
            /// 带区号的手机号。
            /// 国内手机号通常与 purePhoneNumber 一致。
            /// </summary>
            public string phoneNumber { get; set; }

            /// <summary>
            /// 不带区号的手机号。
            /// </summary>
            public string purePhoneNumber { get; set; }

            /// <summary>
            /// 区号。
            /// 中国大陆通常是 86。
            /// </summary>
            public string countryCode { get; set; }

            /// <summary>
            /// 微信水印信息。
            /// </summary>
            public Watermark watermark { get; set; }
        }

        /// <summary>
        /// 微信水印信息。
        /// </summary>
        public class Watermark
        {
            /// <summary>
            /// 数据归属的小程序 appid。
            /// 后端应校验它是否等于当前小程序 appid。
            /// </summary>
            public string appid { get; set; }

            /// <summary>
            /// 数据生成时间戳。
            /// </summary>
            public string timestamp { get; set; }
        }

        /// <summary>
        /// 小程序普通 access_token 返回结果。
        /// </summary>
        public class GetAccessTokenResult
        {
            /// <summary>
            /// 微信返回的 access_token。
            /// </summary>
            public string access_token { get; set; }

            /// <summary>
            /// access_token 有效期，单位秒。
            /// </summary>
            public long expires_in { get; set; }

            /// <summary>
            /// 微信错误码。
            /// 0 表示成功。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息。
            /// </summary>
            public string errmsg { get; set; }
        }

        /// <summary>
        /// 小程序类目数据。
        /// </summary>
        public class GetCategoryDataResult
        {
            /// <summary>
            /// 类目 id。
            /// </summary>
            public long id { get; set; }

            /// <summary>
            /// 类目名称。
            /// </summary>
            public string name { get; set; }
        }

        /// <summary>
        /// 小程序类目接口返回结果。
        /// </summary>
        public class GetCategoryResult
        {
            /// <summary>
            /// 微信错误码。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息。
            /// </summary>
            public string errmsg { get; set; }

            /// <summary>
            /// 类目列表。
            /// </summary>
            public List<GetCategoryDataResult> data { get; set; }
        }

        /// <summary>
        /// 订阅消息模板列表返回结果。
        /// </summary>
        public class GetTemplateListResult
        {
            /// <summary>
            /// 微信错误码。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息。
            /// </summary>
            public string errmsg { get; set; }

            /// <summary>
            /// 模板列表。
            /// </summary>
            public List<GetTemplateListDataResult> data { get; set; }
        }

        /// <summary>
        /// 订阅消息模板数据。
        /// </summary>
        public class GetTemplateListDataResult
        {
            /// <summary>
            /// 个人模板 id。
            /// 发送订阅消息时使用。
            /// </summary>
            public string priTmplId { get; set; }

            /// <summary>
            /// 模板标题。
            /// </summary>
            public string title { get; set; }

            /// <summary>
            /// 模板内容。
            /// </summary>
            public string content { get; set; }

            /// <summary>
            /// 模板示例。
            /// </summary>
            public string example { get; set; }

            /// <summary>
            /// 模板类型。
            /// 2 表示一次性订阅，3 表示长期订阅。
            /// </summary>
            public long type { get; set; }

            /// <summary>
            /// 关键词枚举值列表。
            /// </summary>
            public List<KeywordEnumValueResult> keywordEnumValueList { get; set; }
        }

        /// <summary>
        /// 关键词枚举值。
        /// </summary>
        public class KeywordEnumValueResult
        {
            /// <summary>
            /// 关键词编码。
            /// </summary>
            public string keywordCode { get; set; }

            /// <summary>
            /// 枚举值列表。
            /// </summary>
            public List<string> enumValueList { get; set; }
        }

        /// <summary>
        /// 公共模板标题数据。
        /// </summary>
        public class GetPubTemplateTitleListDataResult
        {
            /// <summary>
            /// 模板标题 id。
            /// </summary>
            public long tid { get; set; }

            /// <summary>
            /// 模板标题。
            /// </summary>
            public string title { get; set; }

            /// <summary>
            /// 模板类型。
            /// </summary>
            public long type { get; set; }

            /// <summary>
            /// 类目 id。
            /// </summary>
            public long categoryId { get; set; }
        }

        /// <summary>
        /// 公共模板标题列表返回结果。
        /// </summary>
        public class GetPubTemplateTitleListResult
        {
            /// <summary>
            /// 微信错误码。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息。
            /// </summary>
            public string errmsg { get; set; }

            /// <summary>
            /// 总数。
            /// </summary>
            public long count { get; set; }

            /// <summary>
            /// 标题列表。
            /// </summary>
            public List<GetPubTemplateTitleListDataResult> data { get; set; }
        }

        /// <summary>
        /// 公共模板关键词数据。
        /// </summary>
        public class GetPubTemplateKeyWordsByIdDataResult
        {
            /// <summary>
            /// 关键词 id。
            /// </summary>
            public long kid { get; set; }

            /// <summary>
            /// 关键词名称。
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// 示例值。
            /// </summary>
            public string example { get; set; }

            /// <summary>
            /// 参数规则。
            /// </summary>
            public string rule { get; set; }
        }

        /// <summary>
        /// 公共模板关键词返回结果。
        /// </summary>
        public class GetPubTemplateKeyWordsByIdResult
        {
            /// <summary>
            /// 微信错误码，小写字段用于接收微信官方 JSON。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息，小写字段用于接收微信官方 JSON。
            /// </summary>
            public string errmsg { get; set; }

            /// <summary>
            /// 兼容旧代码的 errCode 写法。
            /// </summary>
            public long errCode { get => errcode; set => errcode = value; }

            /// <summary>
            /// 兼容旧代码的 errMsg 写法。
            /// </summary>
            public string errMsg { get => errmsg; set => errmsg = value; }

            /// <summary>
            /// 总数。
            /// </summary>
            public long count { get; set; }

            /// <summary>
            /// 关键词列表。
            /// </summary>
            public List<GetPubTemplateKeyWordsByIdDataResult> data { get; set; }
        }

        /// <summary>
        /// 添加订阅消息模板参数。
        /// </summary>
        public class AddTemplateParamter
        {
            /// <summary>
            /// 模板标题 id。
            /// </summary>
            public string tid { get; set; }

            /// <summary>
            /// 关键词 id 列表。
            /// </summary>
            public List<long> kidList { get; set; }

            /// <summary>
            /// 服务场景说明。
            /// </summary>
            public string sceneDesc { get; set; }
        }

        /// <summary>
        /// 添加订阅消息模板返回结果。
        /// </summary>
        public class AddTemplateResult
        {
            /// <summary>
            /// 微信错误码，小写字段用于接收微信官方 JSON。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息，小写字段用于接收微信官方 JSON。
            /// </summary>
            public string errmsg { get; set; }

            /// <summary>
            /// 兼容旧代码的 errCode 写法。
            /// </summary>
            public long errCode { get => errcode; set => errcode = value; }

            /// <summary>
            /// 兼容旧代码的 errMsg 写法。
            /// </summary>
            public string errMsg { get => errmsg; set => errmsg = value; }

            /// <summary>
            /// 添加成功后返回的个人模板 id。
            /// </summary>
            public string priTmplId { get; set; }
        }

        /// <summary>
        /// 删除订阅消息模板返回结果。
        /// </summary>
        public class DeleteTemplateResult
        {
            /// <summary>
            /// 微信错误码，小写字段用于接收微信官方 JSON。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息，小写字段用于接收微信官方 JSON。
            /// </summary>
            public string errmsg { get; set; }

            /// <summary>
            /// 兼容旧代码的 errCode 写法。
            /// </summary>
            public long errCode { get => errcode; set => errcode = value; }

            /// <summary>
            /// 兼容旧代码的 errMsg 写法。
            /// </summary>
            public string errMsg { get => errmsg; set => errmsg = value; }
        }

        /// <summary>
        /// 删除订阅消息模板参数。
        /// </summary>
        public class DeleteTemplateParamter
        {
            /// <summary>
            /// 要删除的个人模板 id。
            /// </summary>
            public string priTmplId { get; set; }

            /// <summary>
            /// 构造删除模板参数。
            /// </summary>
            /// <param name="priTmplId">个人模板 id。</param>
            public DeleteTemplateParamter(string priTmplId)
            {
                this.priTmplId = priTmplId;
            }
        }

        /// <summary>
        /// 订阅消息发送参数。
        /// </summary>
        public class SubscribeMessageSendParamter
        {
            /// <summary>
            /// 接收者 openid。
            /// </summary>
            public string touser { get; set; }

            /// <summary>
            /// 模板 id。
            /// </summary>
            public string template_id { get; set; }

            /// <summary>
            /// 点击模板卡片跳转的小程序页面。
            /// </summary>
            public string page { get; set; }

            /// <summary>
            /// 模板数据。
            /// </summary>
            public object data { get; set; }

            /// <summary>
            /// 跳转小程序版本。
            /// developer 开发版，trial 体验版，formal 正式版。
            /// </summary>
            public string miniprogram_state { get; set; }

            /// <summary>
            /// 语言。
            /// 默认 zh_CN。
            /// </summary>
            public string lang { get; set; }
        }

        /// <summary>
        /// 订阅消息发送返回结果。
        /// </summary>
        public class SubscribeMessageSendResult
        {
            /// <summary>
            /// 微信错误码，小写字段用于接收微信官方 JSON。
            /// </summary>
            public long errcode { get; set; }

            /// <summary>
            /// 微信错误信息，小写字段用于接收微信官方 JSON。
            /// </summary>
            public string errmsg { get; set; }

            /// <summary>
            /// 兼容旧代码的 errCode 写法。
            /// </summary>
            public long errCode { get => errcode; set => errcode = value; }

            /// <summary>
            /// 兼容旧代码的 errMsg 写法。
            /// </summary>
            public string errMsg { get => errmsg; set => errmsg = value; }
        }

        /// <summary>
        /// 获取稳定版小程序 access_token。
        /// 官方接口：https://api.weixin.qq.com/cgi-bin/stable_token
        /// 推荐生产环境使用此接口，并缓存 access_token。
        /// </summary>
        public async Task<AccessTokenResult> GetStableAccessToken(string grant_type, string appid, string secret, bool force_refresh = false)
        {
            if (string.IsNullOrWhiteSpace(appid)) throw Tracexception.Oh("小程序 appid 不能为空");
            if (string.IsNullOrWhiteSpace(secret)) throw Tracexception.Oh("小程序 secret 不能为空");

            var result = await Post("https://api.weixin.qq.com/cgi-bin/stable_token", new
            {
                grant_type = "client_credential",
                appid,
                secret,
                force_refresh
            });

            var output = JsonConvert.DeserializeObject<AccessTokenResult>(result);
            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "获取小程序 stable_token 失败");
            return output;
        }

        /// <summary>
        /// 小程序登录凭证校验。
        /// 前端必须每次调用 wx.login 获取新的 code 后立即传给后端。
        /// </summary>
        public async Task<Code2SessionResult> GetCode2Session(Code2SessionParamter code2SessionParamter)
        {
            if (code2SessionParamter == null) throw Tracexception.Oh("登录凭证校验参数不能为空");

            if (string.IsNullOrWhiteSpace(code2SessionParamter.appid)) code2SessionParamter.appid = appid;
            if (string.IsNullOrWhiteSpace(code2SessionParamter.secret)) code2SessionParamter.secret = secret;
            if (string.IsNullOrWhiteSpace(code2SessionParamter.grant_type)) code2SessionParamter.grant_type = "authorization_code";

            if (string.IsNullOrWhiteSpace(code2SessionParamter.appid)) throw Tracexception.Oh("小程序 appid 不能为空");
            if (string.IsNullOrWhiteSpace(code2SessionParamter.secret)) throw Tracexception.Oh("小程序 secret 不能为空");
            if (string.IsNullOrWhiteSpace(code2SessionParamter.js_code)) throw Tracexception.Oh("小程序登录 code 不能为空");

            var url =
                "https://api.weixin.qq.com/sns/jscode2session" +
                $"?appid={Uri.EscapeDataString(code2SessionParamter.appid)}" +
                $"&secret={Uri.EscapeDataString(code2SessionParamter.secret)}" +
                $"&js_code={Uri.EscapeDataString(code2SessionParamter.js_code)}" +
                $"&grant_type={Uri.EscapeDataString(code2SessionParamter.grant_type)}";

            var result = await Get(url);
            var output = JsonConvert.DeserializeObject<Code2SessionResult>(result);

            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "小程序登录凭证校验失败");
            return output;
        }

        /// <summary>
        /// 小程序登录凭证校验简化方法。
        /// </summary>
        public async Task<Code2SessionResult> GetCode2Session(string js_code, string grant_type = "authorization_code")
        {
            return await GetCode2Session(new Code2SessionParamter(appid, secret, js_code)
            {
                grant_type = grant_type
            });
        }

        /// <summary>
        /// 获取小程序普通 access_token。
        /// 生产环境更建议使用 GetStableAccessToken。
        /// </summary>
        public async Task<GetAccessTokenResult> GetAccessToken(string grant_type = "client_credential")
        {
            if (string.IsNullOrWhiteSpace(appid)) throw Tracexception.Oh("小程序 appid 不能为空");
            if (string.IsNullOrWhiteSpace(secret)) throw Tracexception.Oh("小程序 secret 不能为空");

            var url =
                "https://api.weixin.qq.com/cgi-bin/token" +
                $"?grant_type={Uri.EscapeDataString(grant_type)}" +
                $"&appid={Uri.EscapeDataString(appid)}" +
                $"&secret={Uri.EscapeDataString(secret)}";

            var result = await Get(url);
            var output = JsonConvert.DeserializeObject<GetAccessTokenResult>(result);

            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "获取小程序 access_token 失败");
            return output;
        }

        /// <summary>
        /// 获取小程序手机号。
        /// 注意：这里的 code 是手机号组件返回的 code，不是 wx.login 返回的 js_code。
        /// </summary>
        public async Task<GetPhoneNumberResult> GetMobilePhone(string access_token, string code, string openid = null)
        {
            if (string.IsNullOrWhiteSpace(access_token)) throw Tracexception.Oh("小程序 access_token 不能为空");
            if (string.IsNullOrWhiteSpace(code)) throw Tracexception.Oh("手机号 code 不能为空");

            var url = $"https://api.weixin.qq.com/wxa/business/getuserphonenumber?access_token={Uri.EscapeDataString(access_token)}";

            var result = await Post(url, new
            {
                code
            });

            var output = JsonConvert.DeserializeObject<GetPhoneNumberResult>(result);
            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "获取小程序手机号失败");
            return output;
        }

        /// <summary>
        /// 获取小程序账号类目。
        /// </summary>
        public async Task<GetCategoryResult> GetCategory(string access_token)
        {
            if (string.IsNullOrWhiteSpace(access_token)) throw Tracexception.Oh("小程序 access_token 不能为空");

            var result = await Get($"https://api.weixin.qq.com/wxaapi/newtmpl/getcategory?access_token={Uri.EscapeDataString(access_token)}");
            var output = JsonConvert.DeserializeObject<GetCategoryResult>(result);

            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "获取小程序类目失败");
            return output;
        }

        /// <summary>
        /// 获取小程序公共模板标题列表。
        /// </summary>
        public async Task<GetPubTemplateTitleListResult> GetPubTemplateTitleList(string access_token, string ids, long start = 0, long limit = 30)
        {
            if (string.IsNullOrWhiteSpace(access_token)) throw Tracexception.Oh("小程序 access_token 不能为空");
            if (string.IsNullOrWhiteSpace(ids)) throw Tracexception.Oh("小程序类目 ids 不能为空");

            var url =
                "https://api.weixin.qq.com/wxaapi/newtmpl/getpubtemplatetitles" +
                $"?access_token={Uri.EscapeDataString(access_token)}" +
                $"&ids={Uri.EscapeDataString(ids)}" +
                $"&start={start}" +
                $"&limit={limit}";

            var result = await Get(url);
            var output = JsonConvert.DeserializeObject<GetPubTemplateTitleListResult>(result);

            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "获取小程序公共模板标题失败");
            return output;
        }

        /// <summary>
        /// 获取小程序公共模板关键词。
        /// </summary>
        public async Task<GetPubTemplateKeyWordsByIdResult> GetPubTemplateKeyWordsById(string access_token, string tid)
        {
            if (string.IsNullOrWhiteSpace(access_token)) throw Tracexception.Oh("小程序 access_token 不能为空");
            if (string.IsNullOrWhiteSpace(tid)) throw Tracexception.Oh("模板标题 tid 不能为空");

            var url =
                "https://api.weixin.qq.com/wxaapi/newtmpl/getpubtemplatekeywords" +
                $"?access_token={Uri.EscapeDataString(access_token)}" +
                $"&tid={Uri.EscapeDataString(tid)}";

            var result = await Get(url);
            var output = JsonConvert.DeserializeObject<GetPubTemplateKeyWordsByIdResult>(result);

            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "获取小程序公共模板关键词失败");
            return output;
        }

        /// <summary>
        /// 获取当前小程序账号下的个人模板列表。
        /// </summary>
        public async Task<GetTemplateListResult> GetTemplateList(string access_token)
        {
            if (string.IsNullOrWhiteSpace(access_token)) throw Tracexception.Oh("小程序 access_token 不能为空");

            var result = await Get($"https://api.weixin.qq.com/wxaapi/newtmpl/gettemplate?access_token={Uri.EscapeDataString(access_token)}");
            var output = JsonConvert.DeserializeObject<GetTemplateListResult>(result);

            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "获取小程序个人模板失败");
            return output;
        }

        /// <summary>
        /// 添加小程序订阅消息模板。
        /// </summary>
        public async Task<AddTemplateResult> AddTemplate(string access_token, AddTemplateParamter addTemplateParamter)
        {
            if (string.IsNullOrWhiteSpace(access_token)) throw Tracexception.Oh("小程序 access_token 不能为空");
            if (addTemplateParamter == null) throw Tracexception.Oh("添加模板参数不能为空");

            var result = await Post($"https://api.weixin.qq.com/wxaapi/newtmpl/addtemplate?access_token={Uri.EscapeDataString(access_token)}", addTemplateParamter);
            var output = JsonConvert.DeserializeObject<AddTemplateResult>(result);

            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "添加小程序订阅消息模板失败");
            return output;
        }

        /// <summary>
        /// 删除小程序订阅消息模板。
        /// </summary>
        public async Task<DeleteTemplateResult> DeleteTemplate(string access_token, string priTmplId)
        {
            if (string.IsNullOrWhiteSpace(access_token)) throw Tracexception.Oh("小程序 access_token 不能为空");
            if (string.IsNullOrWhiteSpace(priTmplId)) throw Tracexception.Oh("个人模板 priTmplId 不能为空");

            var result = await Post($"https://api.weixin.qq.com/wxaapi/newtmpl/deltemplate?access_token={Uri.EscapeDataString(access_token)}", new DeleteTemplateParamter(priTmplId));
            var output = JsonConvert.DeserializeObject<DeleteTemplateResult>(result);

            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "删除小程序订阅消息模板失败");
            return output;
        }

        /// <summary>
        /// 发送小程序订阅消息。
        /// </summary>
        public async Task<SubscribeMessageSendResult> Send(string access_token, SubscribeMessageSendParamter subscribeMessageSendParamter)
        {
            if (string.IsNullOrWhiteSpace(access_token)) throw Tracexception.Oh("小程序 access_token 不能为空");
            if (subscribeMessageSendParamter == null) throw Tracexception.Oh("订阅消息参数不能为空");

            var result = await Post($"https://api.weixin.qq.com/cgi-bin/message/subscribe/send?access_token={Uri.EscapeDataString(access_token)}", subscribeMessageSendParamter);
            var output = JsonConvert.DeserializeObject<SubscribeMessageSendResult>(result);

            EnsureMiniProgramSuccess(output?.errcode ?? 0, output?.errmsg, "发送小程序订阅消息失败");
            return output;
        }

        /// <summary>
        /// 微信接口 GET 请求。
        /// </summary>
        public async Task<string> Get(string url)
        {
            try
            {
                using var response = await MiniProgramHttpClient.GetAsync(url);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw Tracexception.Oh($"微信接口 HTTP 请求失败：{(int)response.StatusCode} {response.ReasonPhrase}，响应：{result}");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw Tracexception.Oh($"请求微信接口发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 微信接口 POST JSON 请求。
        /// </summary>
        private static async Task<string> Post(string url, object body = null)
        {
            try
            {
                var json = JsonConvert.SerializeObject(body ?? new { });

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await MiniProgramHttpClient.PostAsync(url, content);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw Tracexception.Oh($"微信接口 HTTP 请求失败：{(int)response.StatusCode} {response.ReasonPhrase}，响应：{result}");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw Tracexception.Oh($"请求微信接口发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 解密获取用户信息。
        /// 不校验签名。
        /// </summary>
        public UserInfo GetUserInfo(string iv, string encryptedData, string session_key)
        {
            try
            {
                var userInfo = JsonConvert.DeserializeObject<UserInfo>(WechatDecrypt(encryptedData, iv, session_key));

                if (userInfo?.watermark != null && !string.IsNullOrWhiteSpace(appid) && userInfo.watermark.appid != appid)
                {
                    throw Tracexception.Oh("微信解密数据 appid 与当前小程序 appid 不一致");
                }

                return userInfo;
            }
            catch (Exception ex)
            {
                throw Tracexception.Oh($"解密微信用户信息发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 解密获取用户信息。
        /// 会先校验 rawData + session_key 的签名。
        /// </summary>
        public UserInfo GetUserInfo(string iv, string encryptedData, string session_key, string rawData, string signature)
        {
            try
            {
                CheckSignature(rawData, session_key, signature);
                return GetUserInfo(iv, encryptedData, session_key);
            }
            catch (Exception ex)
            {
                throw Tracexception.Oh($"解密微信用户信息发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 校验微信小程序数据签名。
        /// </summary>
        public static void CheckSignature(string rawData, string session_key, string signature)
        {
            if (string.IsNullOrWhiteSpace(rawData)) throw Tracexception.Oh("微信 rawData 不能为空");
            if (string.IsNullOrWhiteSpace(session_key)) throw Tracexception.Oh("微信 session_key 不能为空");
            if (string.IsNullOrWhiteSpace(signature)) throw Tracexception.Oh("微信 signature 不能为空");

            var currentSignature = SHA1Encryption(rawData + session_key);

            if (!string.Equals(currentSignature, signature, StringComparison.OrdinalIgnoreCase))
            {
                throw Tracexception.Oh("微信签名校验失败，数据可能已被篡改或 session_key 不匹配");
            }
        }

        /// <summary>
        /// SHA1 加密。
        /// 微信小程序旧版数据签名校验会用到。
        /// </summary>
        public static string SHA1Encryption(string content, Encoding encode = null)
        {
            try
            {
                if (encode == null) encode = Encoding.UTF8;

                using var sha1 = SHA1.Create();

                var bytes = encode.GetBytes(content);
                var hash = sha1.ComputeHash(bytes);

                return BitConverter.ToString(hash).Replace("-", "");
            }
            catch (Exception ex)
            {
                throw Tracexception.Oh($"SHA1 加密失败：{ex.Message}");
            }
        }

        #endregion

        #region 微信小程序数据解密

        /// <summary>
        /// 解密微信小程序 encryptedData。
        /// AES-128-CBC + PKCS7。
        /// </summary>
        public static string WechatDecrypt(string encryptedData, string encryptIv, string sessionKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(encryptedData)) throw Tracexception.Oh("微信 encryptedData 不能为空");
                if (string.IsNullOrWhiteSpace(encryptIv)) throw Tracexception.Oh("微信 iv 不能为空");
                if (string.IsNullOrWhiteSpace(sessionKey)) throw Tracexception.Oh("微信 session_key 不能为空");

                var encryptData = Convert.FromBase64String(encryptedData);
                var key = Convert.FromBase64String(sessionKey);
                var iv = Convert.FromBase64String(encryptIv);

                using var aes = Aes.Create();

                aes.Mode = CipherMode.CBC;
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();

                var decodeByteData = decryptor.TransformFinalBlock(encryptData, 0, encryptData.Length);

                return Encoding.UTF8.GetString(decodeByteData);
            }
            catch (Exception ex)
            {
                throw Tracexception.Oh($"解密微信数据发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 解密微信手机号。
        /// 兼容旧版前端通过 encryptedData + iv 获取手机号的写法。
        /// 新版建议使用 GetMobilePhone 接口。
        /// </summary>
        public static string DecryptPhoneNumber(string encryptedData, string encryptIv, string sessionKey)
        {
            try
            {
                var json = WechatDecrypt(encryptedData, encryptIv, sessionKey);
                var phoneInfo = JsonConvert.DeserializeObject<PhoneInfo>(json);

                return phoneInfo?.phoneNumber ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region 企业微信部门

        /// <summary>
        /// 查询企业微信部门列表。
        /// </summary>
        public string GetDepartmentList()
        {
            var result = MailListApi.GetDepartmentList(accessToken);

            if (result.errcode == 0)
            {
                return JSON.Serialize(result.department);
            }

            throw Tracexception.Oh($"企业微信查询部门失败：{result.errcode}，{result.errmsg}");
        }

        /// <summary>
        /// 创建企业微信部门。
        /// </summary>
        public long CreateDepartment(string name, long parentId, int order, ref string msg, int? id = null, int timeOut = 10000)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    msg = "企业微信部门名称不能为空";
                    return 0;
                }

                var result = MailListApi.CreateDepartment(accessToken, name, parentId, order);

                if (result.errcode == 0)
                {
                    return result.id;
                }

                msg = $"企业微信创建部门失败：{result.errcode}，{result.errmsg}";
                return 0;
            }
            catch (Exception ex)
            {
                msg = $"企业微信创建部门异常：{ex.Message}";
                return 0;
            }
        }

        /// <summary>
        /// 更新企业微信部门。
        /// </summary>
        public bool UpdateDepartment(int? id, string name, int? parentId, int? order, ref string msg, int timeOut = 10000)
        {
            try
            {
                if (id == null)
                {
                    msg = "企业微信部门 id 不能为空";
                    return false;
                }

                var result = MailListApi.UpdateDepartment(accessToken, Convert.ToInt64(id), name, Convert.ToInt64(parentId), Convert.ToInt32(order));

                if (result.errcode == 0)
                {
                    return true;
                }

                msg = $"企业微信更新部门失败：{result.errcode}，{result.errmsg}";
                return false;
            }
            catch (Exception ex)
            {
                msg = $"企业微信更新部门异常：{ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 删除企业微信部门。
        /// </summary>
        public bool DeleteDepartment(int? id, ref string msg)
        {
            try
            {
                if (id == null)
                {
                    msg = "企业微信部门 id 不能为空";
                    return false;
                }

                var result = MailListApi.DeleteDepartment(accessToken, Convert.ToInt64(id));

                if (result.errcode == 0)
                {
                    return true;
                }

                msg = $"企业微信删除部门失败：{result.errcode}，{result.errmsg}";
                return false;
            }
            catch (Exception ex)
            {
                msg = $"企业微信删除部门异常：{ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 获取企业微信部门成员。
        /// </summary>
        public string GetDepartmentMember(long departmentId, int fetchChild = 1)
        {
            var result = MailListApi.GetDepartmentMember(accessToken, departmentId, fetchChild);

            if (result.errcode == 0)
            {
                return result.userlist != null && result.userlist.Count > 0 ? JSON.Serialize(result.userlist) : null;
            }

            throw Tracexception.Oh($"企业微信获取部门成员失败：{result.errcode}，{result.errmsg}");
        }

        #endregion

        #region 企业微信用户

        /// <summary>
        /// 获取企业微信成员详情。
        /// </summary>
        public string GetMember(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw Tracexception.Oh("企业微信成员 userId 不能为空");

            var result = MailListApi.GetMember(accessToken, userId);

            if (result.errcode == 0)
            {
                return JSON.Serialize(result);
            }

            throw Tracexception.Oh($"企业微信获取成员失败：{result.errcode}，{result.errmsg}");
        }

        /// <summary>
        /// 创建企业微信成员。
        /// </summary>
        public bool CreateMember(QYMemberModel member, ref string msg, int timeOut = 10000)
        {
            try
            {
                if (member == null)
                {
                    msg = "企业微信成员信息不能为空";
                    return false;
                }

                var memberCreate = new MemberCreateRequest
                {
                    name = member.name,
                    avatar_mediaid = member.avatar_mediaid,
                    department = member.department,
                    email = member.email,
                    enable = member.enable,
                    english_name = member.english_name,
                    extattr = (Extattr)member.extattr,
                    external_profile = (External_Profile)member.external_profile,
                    gender = member.gender,
                    mobile = member.mobile,
                    order = member.order,
                    position = member.position,
                    telephone = member.telephone,
                    userid = member.userid
                };

                var result = MailListApi.CreateMember(accessToken, memberCreate, timeOut);

                if (result.errcode == 0)
                {
                    return true;
                }

                msg = $"企业微信创建成员失败：{result.errcode}，{result.errmsg}";
                return false;
            }
            catch (Exception ex)
            {
                msg = $"企业微信创建成员异常：{ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 更新企业微信成员。
        /// </summary>
        public bool UpdateMember(QYMemberModel member, ref string msg, int timeOut = 10000)
        {
            try
            {
                if (member == null)
                {
                    msg = "企业微信成员信息不能为空";
                    return false;
                }

                var memberUpdate = new MemberUpdateRequest
                {
                    name = member.name,
                    avatar_mediaid = member.avatar_mediaid,
                    department = member.department,
                    email = member.email,
                    enable = member.enable,
                    english_name = member.english_name,
                    extattr = (Extattr)member.extattr,
                    external_profile = (External_Profile)member.external_profile,
                    gender = member.gender,
                    mobile = member.mobile,
                    order = member.order,
                    position = member.position,
                    telephone = member.telephone,
                    userid = member.userid
                };

                var result = MailListApi.UpdateMember(accessToken, memberUpdate, timeOut);

                if (result.errcode == 0)
                {
                    return true;
                }

                msg = $"企业微信更新成员失败：{result.errcode}，{result.errmsg}";
                return false;
            }
            catch (Exception ex)
            {
                msg = $"企业微信更新成员异常：{ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 删除企业微信成员。
        /// </summary>
        public bool DeleteMember(string userId, string msg = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId)) throw Tracexception.Oh("企业微信成员 userId 不能为空");

                var result = MailListApi.DeleteMember(accessToken, userId);

                if (result.errcode == 0)
                {
                    return true;
                }

                throw Tracexception.Oh($"企业微信删除成员失败：{result.errcode}，{result.errmsg}");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 批量删除企业微信成员。
        /// </summary>
        public bool BatchDeleteMember(string[] useridlist, int timeOut = 10000)
        {
            if (useridlist == null || useridlist.Length == 0) throw Tracexception.Oh("企业微信成员 userId 列表不能为空");

            var result = MailListApi.BatchDeleteMember(accessToken, useridlist, timeOut);

            if (result.errcode == 0)
            {
                return true;
            }

            throw Tracexception.Oh($"企业微信批量删除成员失败：{result.errcode}，{result.errmsg}");
        }

        #endregion

        #region 企业微信消息

        /// <summary>
        /// 发送企业微信文本消息。
        /// </summary>
        public bool SendText(string agentId, string content, string toUser, string toParty = null, string toTag = null, int safe = 0, int timeOut = 10000)
        {
            var result = MassApi.SendText(accessToken, agentId, content, toUser, toParty, toTag, safe, timeOut);

            if (result.errcode == 0)
            {
                return true;
            }

            throw Tracexception.Oh($"企业微信发送文本消息失败：{result.errcode}，{result.errmsg}");
        }

        /// <summary>
        /// 发送企业微信文件消息。
        /// </summary>
        public bool SendFile(string agentId, string mediaId, string toUser, string toParty = null, string toTag = null, int safe = 0, int timeOut = 10000)
        {
            var result = MassApi.SendFile(accessToken, agentId, mediaId, toUser, toParty, toTag, safe, timeOut);

            if (result.errcode == 0)
            {
                return true;
            }

            throw Tracexception.Oh($"企业微信发送文件消息失败：{result.errcode}，{result.errmsg}");
        }

        /// <summary>
        /// 发送企业微信图片消息。
        /// </summary>
        public bool SendImage(string agentId, string mediaId, string toUser, string toParty = null, string toTag = null, int safe = 0, int timeOut = 10000)
        {
            var result = MassApi.SendImage(accessToken, agentId, mediaId, toUser, toParty, toTag, safe, timeOut);

            if (result.errcode == 0)
            {
                return true;
            }

            throw Tracexception.Oh($"企业微信发送图片消息失败：{result.errcode}，{result.errmsg}");
        }

        /// <summary>
        /// 发送企业微信视频消息。
        /// </summary>
        public bool SendVideo(string agentId, string mediaId, string toUser, string toParty = null, string toTag = null, string title = null, string description = null, int safe = 0, int timeOut = 10000)
        {
            var result = MassApi.SendVideo(accessToken, agentId, mediaId, toUser, toParty, toTag, title, description, safe, timeOut);

            if (result.errcode == 0)
            {
                return true;
            }

            throw Tracexception.Oh($"企业微信发送视频消息失败：{result.errcode}，{result.errmsg}");
        }

        /// <summary>
        /// 发送企业微信语音消息。
        /// </summary>
        public bool SendVoice(string agentId, string mediaId, string toUser, string toParty = null, string toTag = null, int safe = 0, int timeOut = 10000)
        {
            var result = MassApi.SendVoice(accessToken, agentId, mediaId, toUser, toParty, toTag, safe, timeOut);

            if (result.errcode == 0)
            {
                return true;
            }

            throw Tracexception.Oh($"企业微信发送语音消息失败：{result.errcode}，{result.errmsg}");
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 统一处理微信小程序接口错误。
        /// </summary>
        private static void EnsureMiniProgramSuccess(long errcode, string errmsg, string title)
        {
            if (errcode == 0)
            {
                return;
            }

            var friendly = errcode switch
            {
                40029 => "code 无效。请确认前端每次登录前都重新调用 wx.login，并且 code 没有被重复使用。",
                40163 => "code 已经被使用。wx.login 的 code 只能使用一次，不能缓存复用。",
                40013 => "appid 无效。请确认后端配置的是小程序 AppID，不是公众号 AppID，也不是企业微信 CorpID。",
                40125 => "secret 无效。请确认后端配置的是小程序 AppSecret。",
                45011 => "接口调用频率过高，请稍后重试。",
                40001 => "access_token 无效或已过期，请重新获取 access_token。",
                41001 => "缺少 access_token 参数。",
                41002 => "缺少 appid 参数。",
                41004 => "缺少 secret 参数。",
                41008 => "缺少 code 参数。",
                _ => errmsg
            };

            throw Tracexception.Oh($"{title}，微信错误码：{errcode}，错误信息：{friendly}");
        }

        #endregion
    }
}