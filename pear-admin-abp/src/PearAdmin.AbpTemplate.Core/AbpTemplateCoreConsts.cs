using System;
using System.IO;
using Abp.Reflection.Extensions;
using PearAdmin.AbpTemplate.Debugging;

namespace PearAdmin.AbpTemplate
{
    public class AbpTemplateCoreConsts
    {
        public const string Version = "1.0.0.0";//版本
        public static DateTime ReleaseDate => LzyReleaseDate.Value;//发行日期
        private static readonly Lazy<DateTime> LzyReleaseDate = new Lazy<DateTime>(() => new FileInfo(typeof(AbpTemplateCoreConsts).GetAssembly().Location).LastWriteTime);

        public const string LocalizationSourceName = "zh-Hans";//本地化源名称
        public const string ConnectionStringName = "Default";//数据库连接字符串
        public const string RedisConnectionStringName = "Redis";//redis数据库连接字符串
        public const bool MultiTenancyEnabled = true;//是否启用多租户
        public const bool AllowTenantsToChangeEmailSettings = false;//是否允许租户更改电子邮件设置

        //表前缀
        public const string TablePrefix_Resource = "Resource";
        public const string TablePrefix_TaskCenter = "Center";
        public const string TablePrefix_Social = "Social";
        public const string TablePrefix_Common = "Common";

        /// <summary>
        /// SimpleStringCipher解密/加密操作的默认密码短语
        /// </summary>
        public static readonly string DefaultPassPhrase = DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "ae4c7fe541bc448f98762eaf37ed3876";


        #region 手机短信模板
        /// <summary>
        /// 手机短信验证码通用模板
        /// </summary>
        public const string SmsTemplateCommonCode = "SMS_119088461";
        #endregion


    }
}
