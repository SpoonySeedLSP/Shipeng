using MongoDB.Driver;

namespace Shipeng.Util.Cache
{
    /// <summary>
    /// MongoDB 数据库连接工厂。
    /// </summary>
    /// <remarks>
    /// 该类型只负责根据主机、端口、库名和超时时间创建 <see cref="IMongoDatabase"/>。
    /// MongoDB.Driver 3.x 已移除旧版 MongoServer/MongoDatabase API，因此这里统一使用新版
    /// IMongoClient/IMongoDatabase，避免继续依赖过时接口。
    /// </remarks>
    public class MongoDb
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _databaseName;
        private readonly TimeSpan _connectTimeout;

        /// <summary>
        /// 初始化 MongoDB 数据库连接工厂。
        /// </summary>
        /// <param name="host">MongoDB 服务所在主机。</param>
        /// <param name="port">MongoDB 服务端口，默认通常为 27017。</param>
        /// <param name="db">需要连接的数据库名称。</param>
        /// <param name="timeOut">连接超时时间，单位为秒；无法解析时默认 60 秒。</param>
        public MongoDb(string host, int port, string db, string timeOut)
        {
            _host = host;
            _port = port;
            _databaseName = db;
            _connectTimeout = int.TryParse(timeOut, out int seconds) && seconds > 0
                ? TimeSpan.FromSeconds(seconds)
                : TimeSpan.FromSeconds(60);
        }

        /// <summary>
        /// 获取 MongoDB 数据库实例。
        /// </summary>
        /// <returns>基于新版驱动的数据库访问入口。</returns>
        public IMongoDatabase GetDataBase()
        {
            var settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(_host, _port),
                ConnectTimeout = _connectTimeout
            };

            return new MongoClient(settings).GetDatabase(_databaseName);
        }
    }
}
