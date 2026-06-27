using Microsoft.Extensions.DependencyInjection;

namespace Shipeng.Data.MongoDB.Extensions
{
    /// <summary>
    /// MongoDb数据库上下文服务扩展
    /// 因为在Programe需要注册，并且要把连接字符串给带过来，
    /// 所以写个扩展方法用于注册（与ef注册时用的builder.ServicesAddDbContext()一个东西，
    /// 只不过人家封装好了的）
    /// </summary>
    public static class MongoDBContextServiceCollectionExtensions
    {
        /// <summary>
        /// 是一个扩展，用于把MongoDB注册到容器中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddMongoDBContext<T>(this IServiceCollection services, Action<MongoDBContextOptions> setupAction) where T : MongoDBContext
        {
            if (services == null)
            { throw new ArgumentNullException(nameof(services)); }
            if (setupAction == null) { throw new ArgumentNullException(nameof(setupAction)); }
            services.Configure(setupAction);
            services.AddScoped<T>();
            return services;
        }
    }
}
