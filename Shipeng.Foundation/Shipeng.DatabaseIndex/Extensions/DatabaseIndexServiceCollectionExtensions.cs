using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Runtime.Loader;

namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// 数据库索引服务注册扩展。
    ///
    /// 这里是 Shipeng.DatabaseIndex 工程给外部使用的唯一入口。
    ///
    /// 使用方式：
    /// services.AddDatabaseIndexEnsure(Configuration);
    ///
    /// 设计目标：
    /// 1. Startup 中不需要手动注册每个业务 Provider。
    /// 2. 自动扫描当前程序已加载的程序集。
    /// 3. 可选扫描程序运行目录下 Shipeng.*.dll，避免漏掉未加载程序集。
    /// 4. 自动注册所有实现 IDatabaseIndexProvider 的类。
    /// </summary>
    public static class DatabaseIndexServiceCollectionExtensions
    {
        /// <summary>
        /// 注册数据库索引自动检查能力。
        /// </summary>
        /// <param name="services">DI 容器。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="markerAssemblies">
        /// 可选程序集标记。
        /// 如果传入，则会额外扫描这些程序集。
        /// 一般不需要传，默认自动扫描即可。
        /// </param>
        public static IServiceCollection AddDatabaseIndexEnsure(
            this IServiceCollection services,
            IConfiguration configuration,
            params Assembly[] markerAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services), "IServiceCollection 不能为空。");

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "IConfiguration 不能为空。");

            services.Configure<DatabaseIndexOptions>(configuration.GetSection("DatabaseIndex"));

            var options = new DatabaseIndexOptions();
            configuration.GetSection("DatabaseIndex").Bind(options);

            var assemblies = GetCandidateAssemblies(options, markerAssemblies);
            var providerTypes = FindProviderTypes(assemblies);

            foreach (var providerType in providerTypes)
            {
                services.TryAddEnumerable(
                    ServiceDescriptor.Transient(typeof(IDatabaseIndexProvider), providerType));
            }

            services.TryAddScoped<IDatabaseIndexEnsureService, DatabaseIndexEnsureService>();
            services.AddHostedService<DatabaseIndexHostedService>();

            return services;
        }

        /// <summary>
        /// 获取候选程序集。
        /// </summary>
        private static List<Assembly> GetCandidateAssemblies(
            DatabaseIndexOptions options,
            Assembly[] markerAssemblies)
        {
            var result = new List<Assembly>();

            var prefixes = options.AssemblyNamePrefixes == null || options.AssemblyNamePrefixes.Count == 0
                ? new List<string> { "Shipeng." }
                : options.AssemblyNamePrefixes;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                if (IsAllowedAssembly(assembly, prefixes))
                    result.Add(assembly);
            }

            if (markerAssemblies != null)
            {
                foreach (var assembly in markerAssemblies)
                {
                    if (assembly != null && !result.Any(x => x.FullName == assembly.FullName))
                        result.Add(assembly);
                }
            }

            if (options.LoadAssembliesFromBaseDirectory)
                LoadAssembliesFromBaseDirectory(result, prefixes);

            return result
                .GroupBy(x => x.FullName)
                .Select(x => x.First())
                .ToList();
        }

        /// <summary>
        /// 从程序运行目录加载业务程序集。
        /// </summary>
        private static void LoadAssembliesFromBaseDirectory(
            List<Assembly> assemblies,
            List<string> prefixes)
        {
            var baseDirectory = AppContext.BaseDirectory;

            if (string.IsNullOrWhiteSpace(baseDirectory) || !Directory.Exists(baseDirectory))
                return;

            var dllFiles = Directory.GetFiles(baseDirectory, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (var dll in dllFiles)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(dll);

                    if (!prefixes.Any(prefix => fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    if (assemblies.Any(x => string.Equals(x.GetName().Name, fileName, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

                    if (!assembly.IsDynamic)
                        assemblies.Add(assembly);
                }
                catch
                {
                    // 这里故意吞掉异常。
                    // 原因：
                    // 1. 有些 dll 可能不是标准 .NET 程序集。
                    // 2. 有些程序集可能已经被加载。
                    // 3. 索引扫描不能因为单个程序集加载失败影响主程序启动。
                }
            }
        }

        /// <summary>
        /// 判断程序集是否允许扫描。
        /// </summary>
        private static bool IsAllowedAssembly(Assembly assembly, List<string> prefixes)
        {
            var name = assembly.GetName().Name;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            return prefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 查找所有数据库索引 Provider 类型。
        /// </summary>
        private static List<Type> FindProviderTypes(List<Assembly> assemblies)
        {
            var result = new List<Type>();

            foreach (var assembly in assemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(x => x != null).Cast<Type>().ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (!IsProviderType(type))
                        continue;

                    result.Add(type);
                }
            }

            return result
                .GroupBy(x => x.FullName)
                .Select(x => x.First())
                .OrderBy(GetProviderOrder)
                .ThenBy(x => x.FullName)
                .ToList();
        }

        /// <summary>
        /// 判断类型是否是有效 Provider。
        /// </summary>
        private static bool IsProviderType(Type type)
        {
            if (type == null)
                return false;

            if (!typeof(IDatabaseIndexProvider).IsAssignableFrom(type))
                return false;

            if (!type.IsClass || type.IsAbstract)
                return false;

            if (type.IsGenericTypeDefinition)
                return false;

            return true;
        }

        /// <summary>
        /// 获取 Provider 排序。
        /// </summary>
        private static int GetProviderOrder(Type type)
        {
            var attr = type
                .GetCustomAttributes(typeof(DatabaseIndexProviderAttribute), false)
                .OfType<DatabaseIndexProviderAttribute>()
                .FirstOrDefault();

            return attr?.Order ?? 0;
        }
    }
}