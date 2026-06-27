namespace Shipeng.Data.MongoDB.Helper
{
    /// <summary>
    /// MongoDB连接信息
    /// </summary>
    public class MongoDBHost
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 库
        /// </summary>
        public string DataBase { get; set; }

        /// <summary>
        /// 集合(表)
        /// </summary>
        public string Collection { get; set; }

    }
}
