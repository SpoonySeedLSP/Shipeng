using System.Text;
using System.Security.Cryptography;

namespace Shipeng.Util.Helper
{
    /// <summary>
    /// MD5工具类
    /// 由于MD5是不可逆的，所以加密之后就无法解密，取用户名和密码时候，需要再加密一边用户输入的数据与数据库中已加密的数据进行比对。
    /// 如果比对结果一致，则可以判定登陆成功
    /// Author：李仕鹏
    /// Date：2020/11/29
    /// </summary>
    public class Md5Helper
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="SourceText">待加密原字符串</param>
        /// <returns></returns>
        public static string MD5Encrypt(string SourceText)
        {
            string tempStr = "";
            MD5 md5 = MD5.Create();
            byte[] data = Encoding.UTF8.GetBytes(SourceText);//将字符编码为一个字节序列 
            byte[] md5data = md5.ComputeHash(data);//计算data字节数组的哈希值 
            md5.Dispose();

            for (int i = 0; i < md5data.Length; i++)
            {
                tempStr += md5data[i].ToString("x").PadLeft(2, '0');
            }
            return tempStr.ToLower();
        }

        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="Source_String"></param>
        /// <returns></returns>
        public static string SHA1_Encrypt(string Source_String)
        {
            byte[] StrRes = Encoding.Default.GetBytes(Source_String);
            HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            StrRes = iSHA.ComputeHash(StrRes);
            StringBuilder EnText = new StringBuilder();
            foreach (byte iByte in StrRes)
            {
                EnText.AppendFormat("{0:x2}", iByte);
            }
            return EnText.ToString();
        }
      
        /// <summary>
        /// HmacSHA256加密
        /// </summary>
        /// <param name="message"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string HmacSHA256(string message, string secret)
        {
            secret = secret ?? "";
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        /// <summary>
        /// 16位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt16(string password)
        {
            var md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(password)), 4, 8);
            t2 = t2.Replace("-", "");
            return t2;
        }

        /// <summary>
        /// 32位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt32(string password)
        {
            string cl = password;
            string pwd = "";
            MD5 md5 = MD5.Create(); //实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
                pwd = pwd + s[i].ToString("X");
            }
            return pwd;
        }

        /// <summary>
        /// 64位的MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt64(string password)
        {
            string cl = password;
            //string pwd = "";
            MD5 md5 = MD5.Create(); //实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            return Convert.ToBase64String(s);
        }

    }
}
