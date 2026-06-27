using AutoMapper;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace Shipeng.Util
{
    /// <summary>
    /// IServiceCollection 扩展方法。
    /// </summary>
    public static partial class Extention
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();

        /// <summary>
        /// 使用 AutoMapper 自动注册带有 MapAttribute 的类型映射。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="configure">调用方额外追加的 AutoMapper 配置。</param>
        public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configure = null)
        {
            List<(Type from, Type[] targets)> maps = new List<(Type from, Type[] targets)>();

            maps.AddRange(GlobalData.AllTypes.Where(x => x.GetCustomAttribute<MapAttribute>() != null)
                .Select(x => (x, x.GetCustomAttribute<MapAttribute>().TargetTypes)));

            MapperConfiguration configuration = new MapperConfiguration(
                cfg =>
                {
                    maps.ForEach(aMap =>
                    {
                        aMap.targets.ToList().ForEach(aTarget =>
                        {
                            cfg.CreateMap(aMap.from, aTarget).IgnoreAllNonExisting(aMap.from, aTarget).ReverseMap();
                        });
                    });

                    cfg.AddMaps(GlobalData.AllFxAssemblies);

                    configure?.Invoke(cfg);
                },
                NullLoggerFactory.Instance);

#if DEBUG
            configuration.AssertConfigurationIsValid();
#endif
            services.AddSingleton(configuration.CreateMapper());

            return services;
        }

        /// <summary>
        /// 自动注入实现 ITransientDependency、IScopedDependency 或 ISingletonDependency 的类型。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>服务集合。</returns>
        public static IServiceCollection AddFxServices(this IServiceCollection services)
        {
            Dictionary<Type, ServiceLifetime> lifeTimeMap = new Dictionary<Type, ServiceLifetime>
            {
                { typeof(ITransientDependency), ServiceLifetime.Transient },
                { typeof(IScopedDependency), ServiceLifetime.Scoped },
                { typeof(ISingletonDependency), ServiceLifetime.Singleton }
            };

            GlobalData.AllTypes.ForEach(aType =>
            {
                lifeTimeMap.ToList().ForEach(aMap =>
                {
                    Type theDependency = aMap.Key;
                    if (!theDependency.IsAssignableFrom(aType) || theDependency == aType || aType.IsAbstract || !aType.IsClass)
                    {
                        return;
                    }

                    List<Type> interfaces = GlobalData.AllTypes
                        .Where(x => x.IsAssignableFrom(aType) && x.IsInterface && x != theDependency)
                        .ToList();

                    if (interfaces.Count > 0)
                    {
                        services.Add(new ServiceDescriptor(aType, aType, aMap.Value));
                        interfaces.ForEach(aInterface =>
                        {
                            services.Add(new ServiceDescriptor(aInterface, serviceProvider =>
                            {
                                return _generator.CreateInterfaceProxyWithTarget(
                                    aInterface,
                                    serviceProvider.GetService(aType),
                                    new CastleInterceptor(serviceProvider));
                            }, aMap.Value));
                        });
                    }
                    else
                    {
                        services.Add(new ServiceDescriptor(aType, aType, aMap.Value));
                    }
                });
            });

            return services;
        }

        /// <summary>
        /// 忽略目标类型中源类型不存在的公开实例属性，避免 AutoMapper 因目标多余属性报错。
        /// </summary>
        /// <param name="expression">映射表达式。</param>
        /// <param name="from">源类型。</param>
        /// <param name="to">目标类型。</param>
        /// <returns>映射表达式。</returns>
        public static IMappingExpression IgnoreAllNonExisting(this IMappingExpression expression, Type from, Type to)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            to.GetProperties(flags).Where(x => from.GetProperty(x.Name, flags) == null).ForEach(aProperty =>
            {
                expression.ForMember(aProperty.Name, opt => opt.Ignore());
            });

            return expression;
        }

        /// <summary>
        /// 忽略目标类型中源类型不存在的公开实例属性，避免 AutoMapper 因目标多余属性报错。
        /// </summary>
        /// <typeparam name="TSource">源类型。</typeparam>
        /// <typeparam name="TDestination">目标类型。</typeparam>
        /// <param name="expression">映射表达式。</param>
        /// <returns>映射表达式。</returns>
        public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            Type from = typeof(TSource);
            Type to = typeof(TDestination);
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            to.GetProperties(flags).Where(x => from.GetProperty(x.Name, flags) == null).ForEach(aProperty =>
            {
                expression.ForMember(aProperty.Name, opt => opt.Ignore());
            });

            return expression;
        }
    }
}
