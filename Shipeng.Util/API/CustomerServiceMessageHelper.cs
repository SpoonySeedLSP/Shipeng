using System.Security.Cryptography;
using System.Text;

namespace Shipeng.Util
{
    public static class CustomerServiceMessageHelper
    {
        /// <summary>
        /// 基于Sha1的自定义加密字符串方法：输入一个字符串，返回一个由40个字符组成的十六进制的哈希散列（字符串）。
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>加密后的十六进制的哈希散列（字符串）</returns>
        public static string CheckSignature(this string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            byte[] data = SHA1.Create().ComputeHash(buffer);

            StringBuilder sb = new StringBuilder();
            foreach (byte t in data)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
