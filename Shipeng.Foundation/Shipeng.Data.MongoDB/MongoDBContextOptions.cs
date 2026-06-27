namespace Shipeng.Data.MongoDB
{
    /// <summary>
    /// MongoDb数据库上下文配置
    /// 生成一个用与存放字符串信息与所要连接的MongoDB数据库名称信息的类
    /// ---对应这里的信息数据是通过后面和ef上下文/dapper上下文在一起的那个类通过构造传来的
    /// </summary>
    public class MongoDBContextOptions
    {
        /// <summary>
        /// 连接MongoDB字符串
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// 连接对应的数据库名称--MongoDB需要指定连接的数据库
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MongoDBContextOptions Value { get { return this; } }
    }
}
