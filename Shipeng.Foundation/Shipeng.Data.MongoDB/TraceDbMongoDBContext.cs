using Microsoft.Extensions.Options;

namespace Shipeng.Data.MongoDB
{
    /// <summary>
    /// mongoDb上下文  
    /// 云南鹰问智慧科技有限公司的实现
    /// </summary>
    public class TraceDbMongoDBContext : MongoDBContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionsAccessor"></param>
        public TraceDbMongoDBContext(IOptions<MongoDBContextOptions> optionsAccessor) : base(optionsAccessor)
        {
        }
    }
}
