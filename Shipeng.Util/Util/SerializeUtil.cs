using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Shipeng.Util.Util
{
    /// <summary>
    /// 序列化及反序列化的辅助类
    /// </summary>
    public sealed class SerializeUtil
    {
        private SerializeUtil()
        {
        }

        /// <summary>
        /// 获取对象的转换为二进制的字节大小
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static long GetByteSize(object obj)
        {
            long result;
            BinaryFormatter bFormatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                bFormatter.Serialize(stream, obj);
                result = stream.Length;
            }
            return result;
        }

        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <param name="obj">待克隆的对象</param>
        /// <returns>克隆的一个新的对象</returns>
        public static object Clone(object obj)
        {
            object cloned = null;
            BinaryFormatter bFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    bFormatter.Serialize(memoryStream, obj);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    cloned = bFormatter.Deserialize(memoryStream);
                }
                catch //(Exception e)
                {
                    ;
                }
            }

            return cloned;
        }

        /// <summary>
        /// 从文件中读取文本内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件的内容</returns>
        public static string ReadFile(string path)
        {
            string content = string.Empty;
            using (StreamReader reader = new StreamReader(path))
            {
                content = reader.ReadToEnd();
            }

            return content;
        }

        /// <summary>
        /// 读取嵌入资源的文本内容
        /// </summary>
        /// <param name="fileWholeName">包含命名空间的嵌入资源文件名路径</param>
        /// <returns>文件中的文本内容</returns>
        public static string ReadFileFromEmbedded(string fileWholeName)
        {
            string result = string.Empty;
            Assembly assembly = Assembly.GetEntryAssembly();
            using (TextReader reader = new StreamReader(assembly.GetManifestResourceStream(fileWholeName)))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }
    }

}
