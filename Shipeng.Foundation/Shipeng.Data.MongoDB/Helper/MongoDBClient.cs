using MongoDB.Driver;

namespace Shipeng.Data.MongoDB.Helper
{
    /// <summary>
    /// MongoDB客户端
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MongoDBClient<T> where T : class
    {
        private MongoDBClient()
        {
        }

        /// <summary>
        /// 获取集合实例
        /// </summary>
        /// <param name="host">连接字符串，库，集合</param>
        /// <returns></returns>
        public static IMongoCollection<T> GetCollectionInstance(MongoDBHost host)
        {
            MongoClient client = new MongoClient(host.ConnectionString);
            var dataBase = client.GetDatabase(host.DataBase);
            return dataBase.GetCollection<T>(host.Collection);
        }
    }
}
