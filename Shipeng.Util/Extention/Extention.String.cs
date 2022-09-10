using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Shipeng.Util
{
    public static partial class Extention
    {
        /// <summary>
        /// Base64解密 注:默认使用UTF8编码
        /// </summary>
        /// <param name="result"> 待解密的密文 </param>
        /// <returns> 解密后的字符串 </returns>
        public static string Base64Decode(this string result)
        {
            return Base64Decode(result, Encoding.UTF8);
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="result"> 待解密的密文 </param>
        /// <param name="encoding"> 解密采用的编码方式，注意和加密时采用的方式一致 </param>
        /// <returns> 解密后的字符串 </returns>
        public static string Base64Decode(this string result, Encoding encoding)
        {
            byte[] bytes = Convert.FromBase64String(result);
            string decode;
            try
            {
                decode = encoding.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }

        /// <summary>
        /// Base64加密 注:默认采用UTF8编码
        /// </summary>
        /// <param name="source"> 待加密的明文 </param>
        /// <returns> 加密后的字符串 </returns>
        public static string Base64Encode(this string source)
        {
            return Base64Encode(source, Encoding.UTF8);
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="source"> 待加密的明文 </param>
        /// <param name="encoding"> 加密采用的编码方式 </param>
        /// <returns> </returns>
        public static string Base64Encode(this string source, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(source);
            string encode;
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = source;
            }
            return encode;
        }

        /// <summary>
        /// Base64Url解码
        /// </summary>
        /// <param name="base64UrlStr"> 使用Base64Url编码后的字符串 </param>
        /// <returns> 解码后的内容 </returns>
        public static string Base64UrlDecode(this string base64UrlStr)
        {
            base64UrlStr = base64UrlStr.Replace('-', '+').Replace('_', '/');
            switch (base64UrlStr.Length % 4)
            {
                case 2:
                    base64UrlStr += "==";
                    break;

                case 3:
                    base64UrlStr += "=";
                    break;
            }
            byte[] bytes = Convert.FromBase64String(base64UrlStr);

            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Base64Url编码
        /// </summary>
        /// <param name="text"> 待编码的文本字符串 </param>
        /// <returns> 编码的文本字符串 </returns>
        public static string Base64UrlEncode(this string text)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(text);
            string base64 = Convert.ToBase64String(plainTextBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

            return base64;
        }

        /// <summary>
        /// 是否为弱密码 注:密码必须包含数字、小写字母、大写字母和其他符号中的两种并且长度大于8
        /// </summary>
        /// <param name="pwd"> 密码 </param>
        /// <returns> </returns>
        public static bool IsWeakPwd(this string pwd)
        {
            if (pwd.IsNullOrEmpty())
            {
                throw new Exception("pwd不能为空");
            }

            string pattern = "(^[0-9]+$)|(^[a-z]+$)|(^[A-Z]+$)|(^.{0,8}$)";
            if (Regex.IsMatch(pwd, pattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 删除Json字符串中键中的@符号
        /// </summary>
        /// <param name="jsonStr"> json字符串 </param>
        /// <returns> </returns>
        public static string RemoveAt(this string jsonStr)
        {
            Regex reg = new Regex("\"@([^ \"]*)\"\\s*:\\s*\"(([^ \"]+\\s*)*)\"");
            string strPatten = "\"$1\":\"$2\"";
            return reg.Replace(jsonStr, strPatten);
        }

        /// <summary>
        /// 将16进制字符串转为Byte数组
        /// </summary>
        /// <param name="str"> 16进制字符串(2个16进制字符表示一个Byte) </param>
        /// <returns> </returns>
        public static byte[] To0XBytes(this string str)
        {
            List<byte> resBytes = new List<byte>();
            for (int i = 0; i < str.Length; i += 2)
            {
                string numStr = $@"{str[i]}{str[i + 1]}";
                resBytes.Add((byte)numStr.ToInt0X());
            }

            return resBytes.ToArray();
        }

        /// <summary>
        /// 将ASCII码形式的字符串转为对应字节数组 注：一个字节一个ASCII码字符
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static byte[] ToASCIIBytes(this string str)
        {
            return str.ToList().Select(x => (byte)x).ToArray();
        }

        /// <summary>
        /// 转为bool
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static bool ToBool(this string str)
        {
            if (IsNullOrEmpty(str))
            {
                return false;
            }
            return bool.Parse(str);
        }

        /// <summary>
        /// string转byte[]
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static byte[] ToBytes(this string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        /// <summary>
        /// string转byte[]
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <param name="theEncoding"> 需要的编码 </param>
        /// <returns> </returns>
        public static byte[] ToBytes(this string str, Encoding theEncoding)
        {
            return theEncoding.GetBytes(str);
        }

        /// <summary>
        /// 转为字节数组
        /// </summary>
        /// <param name="base64Str"> base64字符串 </param>
        /// <returns> </returns>
        public static byte[] ToBytes_FromBase64Str(this string base64Str)
        {
            return Convert.FromBase64String(base64Str);
        }

        /// <summary>
        /// 将Json字符串转为DataTable
        /// </summary>
        /// <param name="jsonStr"> Json字符串 </param>
        /// <returns> </returns>
        public static DataTable ToDataTable(this string jsonStr)
        {
            return jsonStr == null ? null : JsonConvert.DeserializeObject<DataTable>(jsonStr);
        }

        public static bool IsEmpty(this string v)
        {
            if (v == null || v == "" || v.Equals(""))
                return true;
            else
                return false;
        }

        public static string ToStr(this object v)
        {
            return (v == null) ? "" : v.ToString();
        }

        public static string ToStr(this object v, string defaultValue)
        {
            return (v == null) ? defaultValue : v.ToString();
        }

        public static int ToInt(this object v)
        {
            return (v == null || v.ToString() == "") ? 0 : Convert.ToInt32(v.ToString());
        }

        public static decimal ToDecimal(this object v)
        {
            return (v == null || v.ToString() == "") ? 0 : Convert.ToDecimal(v.ToString());
        }

        public static List<string> ReplaceSQLChar(this List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].ReplaceSQLChar();
            }
            return list;
        }

        public static string ReplaceSQLChar(this string str)
        {
            str = ((str == null) ? "" : str.ToString());
            //str = str.Replace(" ", "");
            str = str.Replace("\\s", "");
            str = str.Replace("\n", "");
            str = str.Replace("\b", "");
            str = str.Replace("\f", "");
            str = str.Replace("\r", "");
            str = str.Replace("\t", "");
            str = str.Replace("'", "");
            str = str.Replace("\"", "");
            str = str.Replace("?", "");
            str = str.Replace("<", "");
            str = str.Replace(">", "");
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            str = str.Replace("@", "");
            str = str.Replace("*", "");
            str = str.Replace("&", "");
            str = str.Replace("#", "");
            str = str.Replace("%", "");
            str = str.Replace("$", "");
            str = Regex.Replace(str, "script", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "select", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "from", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "table", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "insert", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "update", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "delete", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "drop", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "into", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "group", "", RegexOptions.IgnoreCase);
            //str = Regex.Replace(str, "order", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "by", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "distinct", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "count", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "truncate", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "mid", "", RegexOptions.IgnoreCase);
            //str = Regex.Replace(str, "char", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "xp_cmdshell", "", RegexOptions.IgnoreCase);
            //str = Regex.Replace(str, "exec master", "", RegexOptions.IgnoreCase);
            //str = Regex.Replace(str, "net localgroup administrators", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "where", "", RegexOptions.IgnoreCase);
            //str = Regex.Replace(str, "and", "", RegexOptions.IgnoreCase);
            //str = Regex.Replace(str, "net user", "", RegexOptions.IgnoreCase);
            //str = Regex.Replace(str, "net", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "script", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "chr", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "master", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "truncate", "", RegexOptions.IgnoreCase);
            //			str = Regex.Replace(str, "declare", "", RegexOptions.IgnoreCase);
            //str = Regex.Replace(str, "limit", "", RegexOptions.IgnoreCase);
            return str;
        }

        /// <summary>
        /// 转换为日期格式
        /// </summary>
        /// <param name="str"> </param>
        /// <returns> </returns>
        public static DateTime ToDateTime(this string str)
        {
            return Convert.ToDateTime(str);
        }

        /// <summary>
        /// 转换为double
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static double ToDouble(this string str)
        {
            return Convert.ToDouble(str);
        }

        /// <summary>
        /// json数据转实体类,仅仅应用于单个实体类，速度非常快
        /// </summary>
        /// <typeparam name="T"> 泛型参数 </typeparam>
        /// <param name="json"> json字符串 </param>
        /// <returns> </returns>
        public static T ToEntity<T>(this string json)
        {
            if (json == null || json == "")
            {
                return default;
            }

            Type type = typeof(T);
            object obj = Activator.CreateInstance(type, null);

            foreach (PropertyInfo item in type.GetProperties())
            {
                PropertyInfo info = obj.GetType().GetProperty(item.Name);
                string pattern;
                pattern = "\"" + item.Name + "\":\"(.*?)\"";
                foreach (Match match in Regex.Matches(json, pattern))
                {
                    switch (item.PropertyType.ToString())
                    {
                        case "System.String": info.SetValue(obj, match.Groups[1].ToString(), null); break;
                        case "System.Int32": info.SetValue(obj, match.Groups[1].ToString().ToInt(), null); ; break;
                        case "System.Int64": info.SetValue(obj, Convert.ToInt64(match.Groups[1].ToString()), null); ; break;
                        case "System.DateTime": info.SetValue(obj, Convert.ToDateTime(match.Groups[1].ToString()), null); ; break;
                    }
                }
            }
            return (T)obj;
        }

        /// <summary>
        /// 将枚举类型的文本转为枚举类型
        /// </summary>
        /// <typeparam name="TEnum"> 枚举类型 </typeparam>
        /// <param name="enumText"> 枚举文本 </param>
        /// <returns> </returns>
        public static TEnum ToEnum<TEnum>(this string enumText) where TEnum : struct
        {
            System.Enum.TryParse(enumText, out TEnum value);

            return value;
        }

        /// <summary>
        /// 转为首字母小写
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static string ToFirstLowerStr(this string str)
        {
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }

        /// <summary>
        /// 转为首字母大写
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static string ToFirstUpperStr(this string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// HMACSHA256算法
        /// </summary>
        /// <param name="text"> 内容 </param>
        /// <param name="secret"> 密钥 </param>
        /// <returns> </returns>
        public static string ToHMACSHA256String(this string text, string secret)
        {
            secret ??= "";
            byte[] keyByte = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(text);
            using HMACSHA256 hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        /// <summary>
        /// string转int
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static int ToInt(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }
            str = str.Replace("\0", "");

            return Convert.ToInt32(str);
        }

        /// <summary>
        /// 二进制字符串转为Int
        /// </summary>
        /// <param name="str"> 二进制字符串 </param>
        /// <returns> </returns>
        public static int ToInt_FromBinString(this string str)
        {
            return Convert.ToInt32(str, 2);
        }

        /// <summary>
        /// 将16进制字符串转为Int
        /// </summary>
        /// <param name="str"> 数值 </param>
        /// <returns> </returns>
        public static int ToInt0X(this string str)
        {
            int num = int.Parse(str, NumberStyles.HexNumber);
            return num;
        }

        /// <summary>
        /// 转为网络终结点IPEndPoint
        /// </summary>
        /// =
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static IPEndPoint ToIPEndPoint(this string str)
        {
            IPEndPoint iPEndPoint;
            try
            {
                string[] strArray = str.ToStringList(':').ToArray();
                string addr = strArray[0];
                int port = Convert.ToInt32(strArray[1]);
                iPEndPoint = new IPEndPoint(IPAddress.Parse(addr), port);
            }
            catch
            {
                iPEndPoint = null;
            }

            return iPEndPoint;
        }

        /// <summary>
        /// 将Json字符串转为JArray
        /// </summary>
        /// <param name="jsonStr"> Json字符串 </param>
        /// <returns> </returns>
        public static JArray ToJArray(this string jsonStr)
        {
            return jsonStr == null ? JArray.Parse("[]") : JArray.Parse(jsonStr.Replace("&nbsp;", ""));
        }

        /// <summary>
        /// 将Json字符串转为JObject
        /// </summary>
        /// <param name="jsonStr"> Json字符串 </param>
        /// <returns> </returns>
        public static JObject ToJObject(this string jsonStr)
        {
            return jsonStr == null ? JObject.Parse("{}") : JObject.Parse(jsonStr.Replace("&nbsp;", ""));
        }

        /// <summary>
        /// 将Json字符串转为List'T'
        /// </summary>
        /// <typeparam name="T"> 对象类型 </typeparam>
        /// <param name="jsonStr"> </param>
        /// <returns> </returns>
        public static List<T> ToList<T>(this string jsonStr)
        {
            return string.IsNullOrEmpty(jsonStr) ? null : JsonConvert.DeserializeObject<List<T>>(jsonStr);
        }

        /// <summary>
        /// 删除重复项(并过滤空项）
        /// </summary>
        /// <param name="list"> </param>
        /// <returns> </returns>
        public static List<string> ToDistinctList(this List<string> list)
        {
            return list.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().Select(p => p.Trim()).ToList();
        }

        /// <summary>
        /// 删除重复项(并过滤空项）
        /// </summary>
        /// <param name="list"> </param>
        /// <returns> </returns>
        public static IEnumerable<string> ToDistinctList(this IEnumerable<string> list)
        {
            return list.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().Select(p => p.Trim());
        }

        /// <summary>
        /// Lst 转字符串 默认 ,分隔
        /// </summary>
        /// <param name="list"> </param>
        /// <param name="i"> </param>
        /// <returns> </returns>
        public static string ToListString(this List<string> list, char i = ',')
        {
            return string.Join(i, list);
        }

        /// <summary>
        /// 字符串  转 Lst默认 ,分隔
        /// </summary>
        /// <param name="list"> </param>
        /// <param name="i"> </param>
        /// <returns> </returns>
        public static List<string> ToStringList(this string list, char i = ',')
        {
            if (list.IsNullOrEmpty())
            {
                return new List<string>();
            }
            return list.Split(i).ToList();
        }

        /// <summary>
        /// list 截取
        /// </summary>
        /// <param name="list"> </param>
        /// <param name="Count"> </param>
        /// <returns> </returns>
        public static List<string> ToCountTakeList(this List<string> list, int Count = 4)
        {
            if (list.Count > Count)
            {
                list = list.Take(Count).ToList();
            }
            return list;
        }

        /// <summary>
        /// string转long
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static long ToLong(this string str)
        {
            str = str.Replace("\0", "");
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            return Convert.ToInt64(str);
        }

        /// <summary>
        /// 转换为MD5加密后的字符串（默认加密为32位）
        /// </summary>
        /// <param name="str"> </param>
        /// <returns> </returns>
        public static string ToMD5String(this string str)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(str);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            md5.Dispose();

            return sb.ToString();
        }

        /// <summary>
        /// 转换为MD5加密后的字符串（16位）
        /// </summary>
        /// <param name="str"> </param>
        /// <returns> </returns>
        public static string ToMD5String16(this string str)
        {
            return str.ToMD5String().Substring(8, 16);
        }

        /// <summary>
        /// 转为MurmurHash
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static uint ToMurmurHash(this string str)
        {
            return MurmurHash2.Hash(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// 计算SHA1摘要 注：默认使用UTF8编码
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static byte[] ToSHA1Bytes(this string str)
        {
            return str.ToSHA1Bytes(Encoding.UTF8);
        }

        /// <summary>
        /// 计算SHA1摘要
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <param name="encoding"> 编码 </param>
        /// <returns> </returns>
        public static byte[] ToSHA1Bytes(this string str, Encoding encoding)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] inputBytes = encoding.GetBytes(str);
            byte[] outputBytes = sha1.ComputeHash(inputBytes);

            return outputBytes;
        }

        /// <summary>
        /// 转为SHA1哈希加密字符串 注：默认使用UTF8编码
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static string ToSHA1String(this string str)
        {
            return str.ToSHA1String(Encoding.UTF8);
        }

        /// <summary>
        /// 转为SHA1哈希
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <param name="encoding"> 编码 </param>
        /// <returns> </returns>
        public static string ToSHA1String(this string str, Encoding encoding)
        {
            byte[] sha1Bytes = str.ToSHA1Bytes(encoding);
            string resStr = BitConverter.ToString(sha1Bytes);
            return resStr.Replace("-", "").ToLower();
        }

        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="str"> 字符串 </param>
        /// <returns> </returns>
        public static string ToSHA256String(this string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] hash = SHA256.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <param name="xmlStr"> XML字符串 </param>
        /// <returns> </returns>
        public static JObject XmlStrToJObject(this string xmlStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            string jsonJsonStr = JsonConvert.SerializeXmlNode(doc);

            return JsonConvert.DeserializeObject<JObject>(jsonJsonStr);
        }

        /// <summary>
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T"> 对象类型 </typeparam>
        /// <param name="xmlStr"> XML字符串 </param>
        /// <returns> </returns>
        public static T XmlStrToObject<T>(this string xmlStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            string jsonJsonStr = JsonConvert.SerializeXmlNode(doc);

            return JsonConvert.DeserializeObject<T>(jsonJsonStr);
        }

        public static string GetEnumDescription(this System.Enum obj)
        {
            return GetEnumDescription(obj, false);
        }

        private static string GetEnumDescription(this System.Enum obj, bool isTop)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            try
            {
                Type _enumType = obj.GetType();
                DescriptionAttribute dna = null;
                if (isTop)
                {
                    dna = (DescriptionAttribute)Attribute.GetCustomAttribute(_enumType, typeof(DescriptionAttribute));
                }
                else
                {
                    FieldInfo fi = _enumType.GetField(System.Enum.GetName(_enumType, obj));

                    dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
                }
                if (dna != null && string.IsNullOrEmpty(dna.Description) == false)
                {
                    return dna.Description;
                }
            }
            catch
            {
                return string.Empty;
            }
            return obj.ToString();
        }

        /// <summary>
        /// 字符串截取
        /// </summary>
        /// <param name="resContent"> 要截取的字符串 </param>
        /// <param name="Length"> 截取长度 默认值1000 </param>
        /// <param name="isText"> 是否添加 ......内容太长已忽略 </param>
        /// <param name="Text"> 提示文字 </param>
        /// <returns> </returns>
        public static string ToStringInterception(this string resContent, int Length = 1000, bool isText = true, string Text = "")
        {
            if (resContent?.Length > Length)
            {
                if (isText)
                {
                    if (Text.Length > 0)
                    {
                        resContent = new string(resContent.Copy(0, Length).ToArray()) + Text;
                    }
                    else
                    {
                        resContent = new string(resContent.Copy(0, Length).ToArray()) + "......内容太长已忽略";
                    }
                }
                else
                {
                    resContent = new string(resContent.Copy(0, Length).ToArray());
                }
            }
            return resContent;
        }

        /// <summary>
        /// 删除日志记录 关联数据 格式new string[] { "Name", "ParentId[W_Sort,Id,Name]", "Contenta" } 需要记录的字段
        /// "Name:表示要记录的字段 ParentId[W_Sort,Id,Name]:表示ParentId关联表W_Sort的id,显示名Name"
        /// </summary>
        /// <param name="str"> </param>
        /// <param name="k"> </param>
        /// <returns> </returns>
        public static List<string> ToStringDeleteRecordAssociation(this List<string> str, string k = "[")
        {
            List<string> addstr = new List<string>();
            if ((string.Join(",", str.ToArray())).Contains(k))//包含  处理
            {
                str.ForEach(x =>
                {
                    addstr.Add(x.Split(k)[0]);
                    //x = x.Split(k)[0];
                });
                str = addstr;
            }
            return str;
        }

        /// <summary>
        /// 字符串转Guid
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Guid ToGuid(this string s)
        {
            return Guid.Parse(s);
        }

        /// <summary>
        /// 根据正则替换
        /// </summary>
        /// <param name="input"></param>
        /// <param name="regex">正则表达式</param>
        /// <param name="replacement">新内容</param>
        /// <returns></returns>
        public static string Replace(this string input, Regex regex, string replacement)
        {
            return regex.Replace(input, replacement);
        }

        #region 检测字符串中是否包含列表中的关键词

        /// <summary>
        /// 检测字符串中是否包含列表中的关键词
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="keys">关键词列表</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static bool Contains(this string s, IEnumerable<string> keys, bool ignoreCase = true)
        {
            if (!keys.Any() || string.IsNullOrEmpty(s))
            {
                return false;
            }

            if (ignoreCase)
            {
                return Regex.IsMatch(s, string.Join("|", keys.Select(Regex.Escape)), RegexOptions.IgnoreCase);
            }

            return Regex.IsMatch(s, string.Join("|", keys.Select(Regex.Escape)));
        }

        /// <summary>
        /// 检测字符串中是否以列表中的关键词结尾
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="keys">关键词列表</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static bool EndsWith(this string s, string[] keys, bool ignoreCase = true)
        {
            if (keys.Length == 0 || string.IsNullOrEmpty(s))
            {
                return false;
            }

            return ignoreCase ? keys.Any(key => s.EndsWith(key, StringComparison.CurrentCultureIgnoreCase)) : keys.Any(s.EndsWith);
        }

        /// <summary>
        /// 检测字符串中是否包含列表中的关键词
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="regex">关键词列表</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static bool RegexMatch(this string s, string regex, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(regex) || string.IsNullOrEmpty(s))
            {
                return false;
            }

            if (ignoreCase)
            {
                return Regex.IsMatch(s, regex, RegexOptions.IgnoreCase);
            }

            return Regex.IsMatch(s, regex);
        }

        /// <summary>
        /// 检测字符串中是否包含列表中的关键词
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="regex">关键词列表</param>
        /// <returns></returns>
        public static bool RegexMatch(this string s, Regex regex)
        {
            return !string.IsNullOrEmpty(s) && regex.IsMatch(s);
        }

        /// <summary>
        /// 判断是否包含符号
        /// </summary>
        /// <param name="str"></param>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public static bool ContainsSymbol(this string str, params string[] symbols)
        {
            return str switch
            {
                null => false,
                "" => false,
                _ => symbols.Any(str.Contains)
            };
        }

        #endregion 检测字符串中是否包含列表中的关键词

        /// <summary>
        /// 判断字符串是否为空或""
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// 字符串掩码
        /// </summary>
        /// <param name="s">字符串</param>
        /// <param name="mask">掩码符</param>
        /// <returns></returns>
        public static string Mask(this string s, char mask = '*')
        {
            if (string.IsNullOrWhiteSpace(s?.Trim()))
            {
                return s;
            }
            s = s.Trim();
            string masks = mask.ToString().PadLeft(4, mask);
            return s.Length switch
            {
                >= 11 => Regex.Replace(s, "(.{3}).*(.{4})", $"$1{masks}$2"),
                10 => Regex.Replace(s, "(.{3}).*(.{3})", $"$1{masks}$2"),
                9 => Regex.Replace(s, "(.{2}).*(.{3})", $"$1{masks}$2"),
                8 => Regex.Replace(s, "(.{2}).*(.{2})", $"$1{masks}$2"),
                7 => Regex.Replace(s, "(.{1}).*(.{2})", $"$1{masks}$2"),
                6 => Regex.Replace(s, "(.{1}).*(.{1})", $"$1{masks}$2"),
                _ => Regex.Replace(s, "(.{1}).*", $"$1{masks}")
            };
        }

        #region IP地址

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static IPAddress MatchInetAddress(this string s, out bool isMatch)
        {
            isMatch = IPAddress.TryParse(s, out IPAddress ip);
            return ip;
        }

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchInetAddress(this string s)
        {
            MatchInetAddress(s, out bool success);
            return success;
        }

        /// <summary>
        /// IP地址转换成数字
        /// </summary>
        /// <param name="addr">IP地址</param>
        /// <returns>数字,输入无效IP地址返回0</returns>
        public static uint IPToID(this string addr)
        {
            if (!IPAddress.TryParse(addr, out IPAddress ip))
            {
                return 0;
            }

            byte[] bInt = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bInt);
            }

            return BitConverter.ToUInt32(bInt, 0);
        }

        /// <summary>
        /// 判断IP地址在不在某个IP地址段
        /// </summary>
        /// <param name="input">需要判断的IP地址</param>
        /// <param name="begin">起始地址</param>
        /// <param name="ends">结束地址</param>
        /// <returns></returns>
        public static bool IpAddressInRange(this string input, string begin, string ends)
        {
            uint current = input.IPToID();
            return current >= begin.IPToID() && current <= ends.IPToID();
        }

        #endregion IP地址

        #region 校验手机号码的正确性

        /// <summary>
        /// 匹配手机号码
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static Match MatchPhoneNumber(this string s, out bool isMatch)
        {
            if (string.IsNullOrEmpty(s))
            {
                isMatch = false;
                return null;
            }
            Match match = Regex.Match(s, @"^((1[3,5,6,8][0-9])|(14[5,7])|(17[0,1,3,6,7,8])|(19[8,9]))\d{8}$");
            isMatch = match.Success;
            return isMatch ? match : null;
        }

        /// <summary>
        /// 匹配手机号码
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchPhoneNumber(this string s)
        {
            MatchPhoneNumber(s, out bool success);
            return success;
        }

        #endregion 校验手机号码的正确性

        /// <summary>
        /// 转换成字节数组
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string @this)
        {
            return Encoding.ASCII.GetBytes(@this);
        }

        /// <summary>
        /// 取字符串前{length}个字
        /// </summary>
        /// <param name="s"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Take(this string s, int length)
        {
            return s.Length > length ? s.Substring(0, length) : s;
        }

        /// <summary>
        /// url地址格式化
        /// </summary>
        /// <param name="obj"> </param>
        /// <returns> </returns>
        public static string ToUrlCanonical(this string obj)
        {
            try
            {
                obj = obj.Split('#')[0];
                obj = obj.Split('?')[0];
                obj = obj.Split('&')[0];
                obj = obj.Split('=')[0];
                return obj.Trim().TrimEnd('/').Replace("p://", "ps://");
            }
            catch
            {
                return obj.Replace("p://", "ps://");
            }
        }

        /// <summary>
        /// 删除-0 的url地址
        /// </summary>
        /// <param name="Url"> </param>
        /// <returns> </returns>
        public static string ToUrl5AString(this string Url)
        {
            if (!Url.IsNullOrEmpty())
            {
                Url = Url.Trim();
                if (Url.Length > 2)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        if (Url.Length > 2)
                        {
                            if (Url.Substring(Url.Length - 2) == "-0")
                            {
                                Url = Url.Substring(0, Url.Length - 2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return Url;
        }

        /// <summary>
        /// 从分隔符开始向尾部截取字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="lastIndexOf">true：从最后一个匹配的分隔符开始截取，false：从第一个匹配的分隔符开始截取，默认：true</param>
        /// <returns>string</returns>
        public static string Substring(this string @this, string separator, bool lastIndexOf = true)
        {
            var startIndex = (lastIndexOf ?
                @this.LastIndexOf(separator, StringComparison.OrdinalIgnoreCase) :
                @this.IndexOf(separator, StringComparison.OrdinalIgnoreCase)) +
                separator.Length;

            var length = @this.Length - startIndex;
            return @this.Substring(startIndex, length);
        }
    }
}