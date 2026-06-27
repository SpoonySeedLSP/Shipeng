using System.Reflection;

namespace Shipeng.EntityFrameworkCore
{
    /// <summary>
    /// Assembly工具类
    /// </summary>
    public class AssemblyHelper
    {
        /// <summary>
        /// 根据指定路径和条件获取程序集
        /// </summary>
        /// <param name="path">程序集路径，默认：AppContext.BaseDirectory</param>
        /// <param name="filter">程序集筛选过滤器</param>
        /// <returns></returns>
        public static Assembly[] GetAssemblies(string path = null, Func<string, bool> filter = null)
        {
            var files = Directory
                            .GetFiles(path ?? AppContext.BaseDirectory, "*.*")
                            .Where(x =>
                                x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                                x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            .Select(x => x
                                .Substring(Path.DirectorySeparatorChar.ToString())
                                .Replace(".dll", "")
                                .Replace(".exe", ""))
                            .Distinct();

            //判断筛选条件是否为空
            if (filter != null)
                files = files.Where(x => filter(x));

            //加载Assembly集
            var assemblies = files.Select(x => Assembly.Load(x));

            return assemblies.ToArray();
        }

        /// <summary>
        /// 获取程序集文件
        /// </summary>
        /// <param name="folderPath">目录路径</param>
        /// <param name="searchOption">检索模式</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAssemblyFiles(string folderPath, SearchOption searchOption)
        {
            return Directory
                    .EnumerateFiles(folderPath, "*.*", searchOption)
                    .Where(s =>
                        s.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                        s.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 根据指定路径和条件获取程序集中所有的类型集合
        /// </summary>
        /// <param name="path">程序集路径，默认：AppContext.BaseDirectory</param>
        /// <param name="filter">程序集筛选过滤器</param>
        /// <returns></returns>
        public static List<Type> GetTypesFromAssembly(string path = null, Func<string, bool> filter = null)
        {
            var types = new List<Type>();
            var assemblies = GetAssemblies(path, filter);

            if (assemblies != null && assemblies.Any())
                foreach (var assembly in assemblies)
                {
                    Type[] typeArray = null;
                    try
                    {
                        typeArray = assembly.GetTypes();
                    }
                    catch
                    {
                    }

                    if (typeArray?.Length > 0)
                        types.AddRange(typeArray);
                }

            return types;
        }
    }
}
