using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections;
using System.Collections.Concurrent;

namespace Shipeng.Util
{
    public static partial class Extensions
    {
        private static readonly ConcurrentDictionary<(Type Source, Type Destination), Lazy<IMapper>> MapperCache = new();

        /// <summary>
        /// Gets a cached mapper for a source/destination pair.
        /// MapperConfiguration creation is expensive, so the utility methods reuse mapper instances per type pair.
        /// </summary>
        private static IMapper GetCachedMapper(Type sourceType, Type destinationType)
        {
            return MapperCache.GetOrAdd((sourceType, destinationType), key =>
                new Lazy<IMapper>(() =>
                {
                    var config = new MapperConfiguration(
                        cfg => cfg.CreateMap(key.Source, key.Destination),
                        NullLoggerFactory.Instance);

                    return config.CreateMapper();
                }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        /// <summary>
        /// 类型映射。
        /// </summary>
        public static T MapTo<T>(this object obj)
        {
            if (obj == null) return default(T);

            return GetCachedMapper(obj.GetType(), typeof(T)).Map<T>(obj);
        }

        /// <summary>
        /// 集合列表类型映射。
        /// </summary>
        public static List<TDestination> MapToList<TDestination>(this IEnumerable source)
        {
            if (source == null) return new List<TDestination>();

            Type sourceType = source.GetType().GetGenericArguments()[0];
            return GetCachedMapper(sourceType, typeof(TDestination)).Map<List<TDestination>>(source);
        }

        /// <summary>
        /// 集合列表类型映射。
        /// </summary>
        public static List<TDestination> MapToList<TSource, TDestination>(this IEnumerable<TSource> source)
        {
            if (source == null) return new List<TDestination>();

            return GetCachedMapper(typeof(TSource), typeof(TDestination)).Map<List<TDestination>>(source);
        }

        /// <summary>
        /// 类型映射。
        /// </summary>
        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
            where TSource : class
            where TDestination : class
        {
            if (source == null) return destination;

            return GetCachedMapper(typeof(TSource), typeof(TDestination)).Map<TDestination>(source);
        }
    }
}
