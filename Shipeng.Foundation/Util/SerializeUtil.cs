using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Shipeng.Util.Util
{
    /// <summary>
    /// 提供通用的序列化、克隆和文本读取辅助方法。
    /// </summary>
    /// <remarks>
    /// 旧版本使用 <c>BinaryFormatter</c> 计算大小和克隆对象。该类型在现代 .NET 中已被废弃，
    /// 对不可信数据反序列化存在严重安全风险，也不利于跨平台和跨语言使用。本实现统一使用
    /// <see cref="JsonSerializer"/>：性能更好、依赖更少、格式可读，更适合公共工具库长期维护。
    /// </remarks>
    public sealed class SerializeUtil
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        private SerializeUtil()
        {
        }

        /// <summary>
        /// 获取对象序列化为 UTF-8 JSON 后的字节数。
        /// </summary>
        /// <param name="obj">需要估算序列化体积的对象；为 <c>null</c> 时返回 0。</param>
        /// <returns>对象 JSON 表示的 UTF-8 字节数。</returns>
        public static long GetByteSize(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return JsonSerializer.SerializeToUtf8Bytes(obj, obj.GetType(), JsonOptions).LongLength;
        }

        /// <summary>
        /// 克隆一个对象。
        /// </summary>
        /// <param name="obj">待克隆对象；为 <c>null</c> 时返回 <c>null</c>。</param>
        /// <returns>通过 JSON 序列化再反序列化得到的新对象实例。</returns>
        /// <remarks>
        /// 该方法适合 DTO、配置对象、简单模型等可 JSON 序列化对象。对于包含循环引用、委托、
        /// 非公开状态或数据库连接等不可序列化资源的对象，不建议使用通用克隆，应由业务类型
        /// 自己提供显式复制方法。
        /// </remarks>
        public static object Clone(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var json = JsonSerializer.Serialize(obj, obj.GetType(), JsonOptions);
            return JsonSerializer.Deserialize(json, obj.GetType(), JsonOptions);
        }

        /// <summary>
        /// 从文件系统读取完整文本内容。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <returns>文件中的全部文本。</returns>
        /// <exception cref="ArgumentException">文件路径为空时抛出。</exception>
        /// <exception cref="FileNotFoundException">指定文件不存在时抛出。</exception>
        public static string ReadFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("文件路径不能为空。", nameof(path));
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// 读取当前入口程序集中的嵌入资源文本。
        /// </summary>
        /// <param name="fileWholeName">完整嵌入资源名称，通常包含默认命名空间和文件名。</param>
        /// <returns>嵌入资源中的全部文本。</returns>
        /// <exception cref="ArgumentException">资源名称为空时抛出。</exception>
        /// <exception cref="FileNotFoundException">未找到指定嵌入资源时抛出。</exception>
        public static string ReadFileFromEmbedded(string fileWholeName)
        {
            if (string.IsNullOrWhiteSpace(fileWholeName))
            {
                throw new ArgumentException("嵌入资源名称不能为空。", nameof(fileWholeName));
            }

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream(fileWholeName)
                ?? throw new FileNotFoundException($"未找到嵌入资源：{fileWholeName}", fileWholeName);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }
    }
}
