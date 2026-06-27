using Shipeng.Util.Helper;
using System.Reflection;

namespace Shipeng.Util
{
    public static class GlobalData
    {
        /// <summary>
        /// 解决方案所有程序集
        /// </summary>
        public static readonly List<Assembly> AllFxAssemblies = new List<Assembly>();

        static GlobalData()
        {
            string[] AllAssemblies = ConfigHelper.GetValue("AllAssemblies").Split(',');

            foreach (string k in AllAssemblies)
            {
                AllFxAssemblies.Add(Assembly.Load(k));
            }
            AllTypes = AllFxAssemblies.SelectMany(x => x.GetTypes()).ToArray();
        }

        /// <summary>
        /// 运行环境
        /// </summary>
        public static bool IsDevelopment { get; set; }

        /// <summary>
        /// 项目名
        /// </summary>
        public const string ProjectName = "Shipeng";

        /// <summary>
        /// 默认机器人名字
        /// </summary>
        public const string DefaultRobot = "Robot";

        /// <summary>
        /// 服务器ip
        /// </summary>
        public const string SpbillCreateIp = "xxx.xx.xx.xxx";

        /// <summary>
        /// 默认产品id
        /// </summary>
        public const string DefaultProductId = "9999999999999999999";

        /// <summary>
        /// 存放Session标志的Cookie名
        /// </summary>
        public static string SessionCookieName => $"{ProjectName}.WXJ.Id";

        /// <summary>
        /// 解决方案所有自定义类
        /// </summary>
        public static readonly Type[] AllTypes;

        /// <summary>
        /// 超级管理员UserIId
        /// </summary>
        public const string ADMINID = "Admin";

        /// <summary>
        /// 默认小程序AppId--xxxxxxx的服务
        /// </summary>
        public const string AppletId = "wxa70c552ad6ed690a";

        /// <summary>
        ///默认字段
        /// </summary>
        public static List<string> IgnoreProperties = new() { "Id", "CreateTime", "CreatorId", "CreatorRealName", "Deleted", "CompanyId" };//修改字段强制不记录列

        /// <summary>
        ///视图  API,PC,M,MIP
        /// </summary>
        public static string Views = ConfigHelper.GetValue("Views");

        /// <summary>
        /// 网站文件根路径
        /// </summary>
        public static string WebRootPath => Directory.GetCurrentDirectory();

        /// <summary>
        /// 资源文件目录wwwroot
        /// </summary>
        public static string WebRootPathWwwroot => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        /// <summary>
        /// 请求频率限制
        /// </summary>
        public static RequestRestrictionDTO RequestRes { get; } = ConfigHelper.GetObjectValue<RequestRestrictionDTO>("RequestRestriction");

        /// <summary>
        /// 默认资源文件路径
        /// </summary>
        public static string WebRootUrl { get; } = ConfigHelper.GetValue("WebRootUrl");

        /// <summary>
        /// 后台接口地址
        /// </summary>
        public static string IntelFaceurl { get; } = ConfigHelper.GetValue("IntelFaceurl");

        /// <summary>
        /// 端口号
        /// </summary>
        public static int WebSocketServer { get; } = Convert.ToInt32(ConfigHelper.GetValue("WebSocketServer"));

        /// <summary>
        /// 出现系统错误 是否返回值 错误提示 true:返回错误提示,  false:返回系统繁忙加 "【8dK149】" 临时错误码
        /// </summary>
        public static bool IsErrorPrompt { get; } = Convert.ToBoolean(ConfigHelper.GetValue("IsErrorPrompt"));

        /// <summary>
        /// 是否截取传入body值
        /// </summary>
        public static bool IsCutOutBody { get; } = Convert.ToBoolean(ConfigHelper.GetValue("IsCutOutBody"));

        /// <summary>
        ///提前生成的小程序码数量
        /// </summary>
        public static int WeChatAppletQR { get; } = Convert.ToInt32(ConfigHelper.GetValue("WeChatAppletQR"));

        /// <summary>
        /// 上传视频格式
        /// </summary>
        public static string[] UploadVideo { get; } = ConfigHelper.GetValue("UploadVideo").Split(',');//上传的文件格式;

        /// <summary>
        /// 上传图片格式
        /// </summary>
        public static string[] UploadImg { get; } = ConfigHelper.GetValue("UploadImg").Split(',');//上传的文件格式;

        /// <summary>
        /// 上传文件格式
        /// </summary>
        public static string[] UploadFile { get; } = ConfigHelper.GetValue("UploadFile").Split(',');//上传的文件格式;

        /// <summary>
        /// 上传音频格式
        /// </summary>
        public static string[] UploadMusic { get; } = ConfigHelper.GetValue("UploadMusic").Split(',');//上传音频格式;

        /// <summary>
        /// 极光Key
        /// </summary>
        public static string JPushKey = ConfigHelper.GetValue("JPushKey");

        /// <summary>
        /// 设置记录执行SQL的最小执行时长，默认50ms
        /// </summary>
        public static int MinCommandElapsedMilliseconds { get; } = Convert.ToInt32(ConfigHelper.GetValue("MinCommandElapsedMilliseconds"));

        #region 鼎薪宝

        /// <summary>
        /// url
        /// </summary>
        public static string DingXinBaoUrl = "http://topen.dingshuimao.com/";

        /// <summary>
        /// AESkey
        /// </summary>
        public static string DingXinBaoAesKey = "62549337F8CE94309B3399710732C4E9";

        /// <summary>
        /// AppId
        /// </summary>
        public static string DingXinBaoAppId = "20210714095510333528";

        /// <summary>
        /// 应用公钥
        /// </summary>
        public static string DingXinBaoAppKey = "202107140955106143";

        /// <summary>
        /// 系统公钥
        /// </summary>
        public static string DingXinBaoSystemKey = "OTE1MzAxMDJNQTZRQjE5SjdIXzIwMjEwNzE0MDk1NTEwNjE0Mw==";

        #endregion 鼎薪宝

        #region 鼎薪宝 新版

        //应用详情
        //应用名称:
        //xxxxxxxx有限公司
        //AppId:
        //20210913104304007602
        //状态:
        //已生效
        //沙箱环境配置
        //回调地址:
        //应用公钥:
        //202109131043045580
        //系统公钥:
        //OTE1MzAxMDJNQTZRQjE5SjdIXzIwMjEwOTEzMTA0MzA0NTU4MA==
        //沙箱环境地址
        //http://topen.dingshuimao.com/
        //AES秘钥:
        //E436B7B257EFE34EB94E653CAF1E8827
        /// <summary>
        /// url
        /// </summary>
        public static string XinDingXinBaoUrl = "http://topen.dingshuimao.com/";

        /// <summary>
        /// AESkey
        /// </summary>
        public static string XinDingXinBaoAesKey = "E436B7B257EFE34EB94E653CAF1E8827";

        /// <summary>
        /// AppId
        /// </summary>
        public static string XinDingXinBaoAppId = "20210913104304007602";

        /// <summary>
        /// 应用公钥
        /// </summary>
        public static string XinDingXinBaoAppKey = "202109131043045580";

        /// <summary>
        /// 系统公钥
        /// </summary>
        public static string XinDingXinBaoSystemKey = "OTE1MzAxMDJNQTZRQjE5SjdIXzIwMjEwOTEzMTA0MzA0NTU4MA==";

        /// <summary>
        /// 公司名称
        /// </summary>
        public const string XinDingXinBaoCompanyName = "xxxxxxx有限公司";

        /// <summary>
        /// 统一社会信用代码
        /// </summary>
        public const string XinDingXinBaoCodes = "91530102MA6QB19J7H";

        #endregion 鼎薪宝 新版

        #region APP包的id

        /// <summary>
        /// 案场管家安卓AppId
        /// </summary>
        public const string CaseCourtAndroid = "";

        /// <summary>
        /// 案场管家苹果AppId
        /// </summary>
        public const string CaseCourtIOS = "1584589348";

        /// <summary>
        /// 全民销冠安卓AppId
        /// </summary>
        public const string ChampionAndroid = "";

        /// <summary>
        /// 全民销冠苹果AppId
        /// </summary>
        public const string ChampionIOS = "";

        #endregion APP包的id

        #region 微信支付

        /// <summary>
        /// 微信客服Token
        /// </summary>
        public const string CustomerServiceToken = "Xsbnldhcy123";

        /// <summary>
        /// 商户号id--xxxxx
        /// </summary>
        public const string MchId = "1628801658";

        /// <summary>
        /// 商户密钥 APIV2
        /// </summary>
        public const string MchKey = "bb625bbab068c72c7a3e681029b38dc5";

        /// <summary>
        /// 商户密钥 APIV3
        /// </summary>
        public const string MchKeyV3 = "bb625bbab068c72c7a3e681029b38dc5";

        #endregion 微信支付

        /// <summary>
        /// 用户默认等级图片
        /// </summary>
        public static string UserLevelUserImg = "https://i.ynyzjj.com/Image/mine/normaluser.png";

        /// <summary>
        /// /经纪人默认等级图片
        /// </summary>
        public static string UserLevelAgentImg = "https://i.ynyzjj.com/Image/mine/level.png";

        /// <summary>
        /// 活动谢谢惠顾默认图片
        /// </summary>

        public static string VoteProductImg = "https://i.ynyzjj.com/Image/defaulproduct.png";

        /// <summary>
        /// 企业付款到零钱默认限制金额
        /// </summary>

        public static int DefaultRedMoney = Convert.ToInt32(ConfigHelper.GetValue("DefaultRedMoney"));

        /// <summary>
        /// 台账模板路径
        /// </summary>
        public static string ParameterDownloadTemplate = WebRootUrl + "/Upload/Export/Parameter.xlsx";
    }
}
