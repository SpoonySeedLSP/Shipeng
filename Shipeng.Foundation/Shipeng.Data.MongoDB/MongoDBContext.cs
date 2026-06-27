using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Shipeng.Data.MongoDB
{
    /// <summary>
    /// MongoDb数据库上下文
    /// 根据MongoDBContextOptions类中的信息，配置上下文，起连接，取连接的数据库
    /// --这样目的就是实现我们为什么在ef中直接用上下文.表.add就可以对对应数据库，对应表实现添加
    /// </summary>
    public class MongoDBContext
    {
        //此构造方法主要目的是接连接字符串的配置和要操作哪个数据库
        //安装包MongoDB.Driver
        private MongoClient _mongoClent;//mongodb的连接
        private readonly MongoDBContextOptions _options;  //使用强类型来接受配置信息
        private IMongoDatabase _database;

        /// <summary>
        /// 构造函数中接收传过来的配置信息(连接字符串+操作的对应数据库)options
        /// </summary>
        /// <param name="optionsAccessor"></param>
        protected MongoDBContext(IOptions<MongoDBContextOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
            //通过options中存放的连接字符串, 起连接
            _mongoClent = new MongoClient(_options.Configuration);
            //通过options中存放的数据库名称,获取数据库
            _database = _mongoClent.GetDatabase(_options.DatabaseName); 
        }

        /// <summary>
        /// 获取mongodb 的连接
        /// </summary>
        public MongoClient MongoClent { get { return _mongoClent; } }

        /// <summary>
        /// 获取连接的数据库
        /// </summary>
        public IMongoDatabase Database { get { return _database; } }

    }
}
