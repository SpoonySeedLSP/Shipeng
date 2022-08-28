using Microsoft.Extensions.Configuration;

namespace Shipeng.Util.Helper
{
    /// <summary>
    /// 配置文件帮助类
    /// </summary>
    public static class ConfigHelper
    {
        private static IConfiguration _config;
        private static readonly object _lock = new object();

        public static IConfiguration Configuration
        {
            get
            {
                if (_config == null)
                {
                    lock (_lock)
                    {
                        if (_config == null)
                        {
                            IConfigurationBuilder builder = new ConfigurationBuilder()
                                .SetBasePath(AppContext.BaseDirectory)
                                .AddJsonFile("appsettings.json");
                            _config = builder.Build();
                        }
                    }
                }

                return _config;
            }
            set => _config = value;
        }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="nameOfCon"> 连接字符串名 </param>
        /// <returns> </returns>
        public static string GetConnectionString(string nameOfCon)
        {
            string key = Configuration.GetConnectionString(nameOfCon);
            return key;
        }

        /// <summary>
        /// 获取对象配置文件
        /// </summary>
        /// <param name="key"> key </param>
        /// <returns> </returns>
        public static T GetObjectValue<T>(string key)
        {
            return Configuration.GetSection(key).Get<T>();
        }

        /// <summary>
        /// 从AppSettings获取key的值
        /// </summary>
        /// <param name="key"> key </param>
        /// <returns> </returns>
        public static string GetValue(string key)
        {
            return Configuration[key];
        }
    }
}