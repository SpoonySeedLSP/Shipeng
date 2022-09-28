using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Shipeng.Util
{
    public static partial class Extention
    {
        private static BindingFlags _bindingFlags { get; }
            = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        /// <summary>
        /// 可转换类型字典
        /// </summary>
        private static readonly Dictionary<Type, Func<object, object>> ConvertDictionary = new Dictionary<Type, Func<object, object>>();

        /// <summary>
        /// 对象是否为空或者空字符串
        /// </summary>
        /// <param name="inputObj"></param>
        /// <returns></returns>
        public static bool IsNullOrEmptyString(this object inputObj)
        {
            switch (inputObj)
            {
                case null:
                    return true;
                case string inputStr:
                    return string.IsNullOrEmpty(inputStr);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 对象是否为空或者空或者空格字符串
        /// </summary>
        /// <param name="inputObj"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpaceString(this object inputObj)
        {
            switch (inputObj)
            {
                case null:
                    return true;
                case string inputStr:
                    return string.IsNullOrWhiteSpace(inputStr);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 判断是否为Null或者空
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static bool IsNullOrEmpty(this object obj)
        {
            if (obj == null)
            {
                return true;
            }
            else
            {
                return string.IsNullOrEmpty(obj.ToString());
            }
        }

        /// <summary>
        /// 判断是否为Null或者空
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static bool IsNullOrWhiteSpace(this object obj)
        {
            if (obj == null)
            {
                return true;
            }
            else
            {
                return string.IsNullOrWhiteSpace(obj.ToString());
            }
        }

        /// <summary>
        /// 属性是否包含
        /// </summary>
        /// <param name="leftModel"></param>
        /// <param name="rightModel"></param>
        /// <param name="maps"></param>
        /// <returns></returns>
        public static bool PropertyContain(this object leftModel, object rightModel, Dictionary<string, Func<bool>> maps = null)
        {
            Type aType = leftModel.GetType();
            Type bType = rightModel.GetType();
            foreach (PropertyInfo aProperty in aType.GetProperties())
            {
                if (maps != null && maps.ContainsKey(aProperty.Name))
                {
                    bool mapResult = maps[aProperty.Name].Invoke();
                    if (!mapResult) return false;
                }
                else
                {
                    PropertyInfo bProperty = bType.GetProperty(aProperty.Name);
                    if (bProperty == null || aProperty.PropertyType != bProperty.PropertyType) return false;
                    object aValue = aProperty.GetValue(leftModel);
                    object bValue = bProperty.GetValue(rightModel);
                    if (aValue != bValue) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断是否为数字
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static bool IsNumeric(this object obj)
        {
            if (obj == null)
            {
                return true;
            }

            return Regex.IsMatch(obj.ToString(), @"^(-?\d+)(\.\d+)?$");
        }

        /// <summary>
        /// 判断是否为数字
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static bool IsNumericOrEmpty(this object obj)
        {
            if (obj.IsNullOrWhiteSpace())
            {
                return true;
            }

            return Regex.IsMatch(obj.ToString(), @"^(-?\d+)(\.\d+)?$");
        }

        /// <summary>
        /// 判断是否为yyyy/MM/dd格式日期
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static bool IsDate(this object obj)
        {
            if (obj.IsNullOrWhiteSpace())
            {
                return true;
            }
            return Regex.IsMatch(obj.ToString(), @"^[0-9]{4}/[0-9]{1,2}/[0-9]{1,2}$");
        }

        /// <summary>
        /// 判断是否为 dd-MM月-yyyy 格式日期
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static bool IsExcelDate(this object obj)
        {
            if (obj.IsNullOrWhiteSpace())
            {
                return true;
            }
            return Regex.IsMatch(obj.ToString(), @"^[0-9]{1,2}-[0-9]{1,2}月-[0-9]{4}$");
        }

        /// <summary>
        /// 判断是否为yyyy/MM/dd格式日期
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static bool IsTime(this object obj)
        {
            if (obj.IsNullOrWhiteSpace())
            {
                return true;
            }
            return Regex.IsMatch(obj.ToString(), @"^[0-9]{2}:[0-9]{2}$");
        }

        /// <summary>
        /// 判断是否为yyyy/MM/dd HH:mm格式日期
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static bool IsDateTimeOrEmpty(this object obj)
        {
            if (obj.IsNullOrWhiteSpace())
            {
                return true;
            }
            return Regex.IsMatch(obj.ToString(), @"^[0-9]{4}/[0-9]{1,2}/[0-9]{1,2}( [0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2})?)?$");
        }

        /// <summary>
        /// 判断是否提供到特定类型的转换
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool CanConvertTo(this object obj, Type targetType)
        {
            return ConvertDictionary.ContainsKey(targetType);
        }

        /// <summary>
        /// 实体类转json数据，速度快
        /// </summary>
        /// <param name="t"> 实体类 </param>
        /// <returns> </returns>
        public static string EntityToJson(this object t)
        {
            if (t == null)
            {
                return null;
            }

            string jsonStr = "";
            jsonStr += "{";
            PropertyInfo[] infos = t.GetType().GetProperties();
            for (int i = 0; i < infos.Length; i++)
            {
                jsonStr = jsonStr + "\"" + infos[i].Name + "\":\"" + infos[i].GetValue(t).ToString() + "\"";
                if (i != infos.Length - 1)
                {
                    jsonStr += ",";
                }
            }
            jsonStr += "}";
            return jsonStr;
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <typeparam name="T"> 类型 </typeparam>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static T DeepClone<T>(this T obj) where T : class
        {
            if (obj == null)
            {
                return null;
            }

            return obj.ToJson().ToObject<T>();
        }

        /// <summary>
        /// 将对象序列化为XML字符串
        /// </summary>
        /// <typeparam name="T"> 对象类型 </typeparam>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static string ToXmlStr<T>(this T obj)
        {
            string jsonStr = obj.ToJson();
            System.Xml.XmlDocument xmlDoc = JsonConvert.DeserializeXmlNode(jsonStr);
            string xmlDocStr = xmlDoc.InnerXml;

            return xmlDocStr;
        }

        /// <summary>
        /// 将对象序列化为XML字符串
        /// </summary>
        /// <typeparam name="T"> 对象类型 </typeparam>
        /// <param name="obj"> 对象 </param>
        /// <param name="rootNodeName"> 根节点名(建议设为xml) </param>
        /// <returns> </returns>
        public static string ToXmlStr<T>(this T obj, string rootNodeName)
        {
            string jsonStr = obj.ToJson();
            System.Xml.XmlDocument xmlDoc = JsonConvert.DeserializeXmlNode(jsonStr, rootNodeName);
            string xmlDocStr = xmlDoc.InnerXml;

            return xmlDocStr;
        }

        /// <summary>
        /// 是否拥有某属性
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <param name="propertyName"> 属性名 </param>
        /// <returns> </returns>
        public static bool ContainsProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName, _bindingFlags) != null;
        }

        /// <summary>
        /// 获取某属性值
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <param name="propertyName"> 属性名 </param>
        /// <returns> </returns>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName, _bindingFlags).GetValue(obj);
        }

        /// <summary>
        /// 设置某属性值
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <param name="propertyName"> 属性名 </param>
        /// <param name="value"> 值 </param>
        /// <returns> </returns>
        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName, _bindingFlags).SetValue(obj, value);
        }

        /// <summary>
        /// 是否拥有某字段
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <param name="fieldName"> 字段名 </param>
        /// <returns> </returns>
        public static bool ContainsField(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, _bindingFlags) != null;
        }

        /// <summary>
        /// 获取某字段值
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <param name="fieldName"> 字段名 </param>
        /// <returns> </returns>
        public static object GetGetFieldValue(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, _bindingFlags).GetValue(obj);
        }

        /// <summary>
        /// 设置某字段值
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <param name="fieldName"> 字段名 </param>
        /// <param name="value"> 值 </param>
        /// <returns> </returns>
        public static void SetFieldValue(this object obj, string fieldName, object value)
        {
            obj.GetType().GetField(fieldName, _bindingFlags).SetValue(obj, value);
        }

        /// <summary>
        /// 获取某字段值
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <param name="methodName"> 方法名 </param>
        /// <returns> </returns>
        public static MethodInfo GetMethod(this object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName, _bindingFlags);
        }

        /// <summary>
        /// 改变实体类型
        /// </summary>
        /// <param name="obj"> 对象 </param>
        /// <param name="targetType"> 目标类型 </param>
        /// <returns> </returns>
        public static object ChangeType(this object obj, Type targetType)
        {
            return obj.ToJson().ToObject(targetType);
        }

        /// <summary>
        /// 改变实体类型
        /// </summary>
        /// <typeparam name="T"> 目标泛型 </typeparam>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static T ChangeType<T>(this object obj)
        {
            return obj.ToJson().ToObject<T>();
        }

        /// <summary>
        /// 改变类型
        /// </summary>
        /// <param name="obj"> 原对象 </param>
        /// <param name="targetType"> 目标类型 </param>
        /// <returns> </returns>
        public static object ChangeType_ByConvert(this object obj, Type targetType)
        {
            object resObj;
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                NullableConverter newNullableConverter = new NullableConverter(targetType);
                resObj = newNullableConverter.ConvertFrom(obj);
            }
            else
            {
                resObj = Convert.ChangeType(obj, targetType);
            }

            return resObj;
        }

        /// <summary>
        /// 数字转大写
        /// </summary>
        /// <param name="inputNum"> </param>
        /// <returns> </returns>
        public static string SetNumberForChanese(this int inputNum)
        {
            string[] strArr = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", };
            string tmpVal = string.Empty;
            if (inputNum < 10)
            {
                tmpVal = strArr[inputNum];
            }
            else
            {
                tmpVal = inputNum.ToString();
            }
            return tmpVal;
        }

        /// <summary>
        /// 切割骆驼命名式字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string[] SplitCamelCase(this string str)
        {
            if (str == null) return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(str)) return new string[] { str };
            if (str.Length == 1) return new string[] { str };

            return Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})")
                .Where(u => u.Length > 0)
                .ToArray();
        }

        #region 获取url

        public static string GetAbsoluteUri(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }

        public static string GetPathBaseUri(this HttpRequest request)
        {
            return new StringBuilder()
                //.Append(request.Scheme)
                //.Append("://")
                //.Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }

        public static string GethttpHostUri(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                //.Append(request.PathBase)
                //.Append(request.Path)
                //.Append(request.QueryString)
                .ToString();
        }

        public static string GetHostUri(this HttpRequest request)
        {
            return new StringBuilder()
                //.Append(request.Scheme)
                //.Append("://")
                .Append(request.Host)
                //.Append(request.PathBase)
                //.Append(request.Path)
                //.Append(request.QueryString)
                .ToString();
        }

        #endregion 获取url

        /// <summary>
        /// 深复制
        /// </summary>
        /// <typeparam name="T"> 类型 </typeparam>
        /// <param name="obj"> 对象 </param>
        /// <returns> </returns>
        public static List<T> ToObjectListObject<T>(this T obj)
        {
            if (obj.IsNullOrEmpty())
            {
                return null;
            }
            List<T> list = new List<T>
            {
                obj
            };
            return list;
        }

        /// <summary>
        /// 转换到特定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this object obj)
        {
            return (T)ConvertTo(obj, typeof(T));
        }
     
        /// <summary>
        /// 转换到特定类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object ConvertTo(this object obj, Type targetType)
        {
            if (obj == null) return !targetType.IsValueType ? (object)null : throw new ArgumentNullException(nameof(obj), "不能将null转换为" + targetType.Name);
            if (obj.GetType() == targetType || targetType.IsInstanceOfType(obj)) return obj;
            if (ConvertDictionary.ContainsKey(targetType)) return ConvertDictionary[targetType](obj);
            try
            {
                return Convert.ChangeType(obj, targetType);
            }
            catch
            {
                throw new ShipengConvertException("未实现到" + targetType.Name + "的转换");
            }
        }

        /// <summary>
        /// 克隆对象(Json序列化)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns></returns>
        public static T CloneByJson<T>(this T inputObj)
        {
            string jsonStr = inputObj.ToJson();
            return jsonStr.JsonToObject<T>();
        }

        /// <summary>
        /// 克隆对象(XML序列化)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns>克隆的对象</returns>
        public static T CloneByXml<T>(this T inputObj)
        {
            Type tType = inputObj.GetType();
            Attribute attr = tType.GetCustomAttribute(typeof(SerializableAttribute));
            if (attr == null) throw new ShipengConvertException("未标识为可序列化");
            object resM;
            using (var ms = new MemoryStream())
            {
                var xml = new XmlSerializer(typeof(T));
                xml.Serialize(ms, inputObj);
                ms.Seek(0, SeekOrigin.Begin);
                resM = xml.Deserialize(ms);
                ms.Close();
            }
            return (T)resM;
        }
        /// <summary>
        /// 克隆对象(反射)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns>克隆的对象</returns>
        public static T CloneByReflex<T>(this T inputObj)
        {
            Type tType = inputObj.GetType();
            var resM = (T)Activator.CreateInstance(tType);
            PropertyInfo[] pis = tType.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                object piValue = pi.GetValue(inputObj);
                if (piValue == null) continue;
                pi.SetValue(resM, piValue is ValueType ? piValue : Clone(piValue));
            }
            return resM;
        }
        /// <summary>
        /// 克隆对象(二进制序列化)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns>克隆的对象</returns>
        public static T CloneBySerializable<T>(this T inputObj)
        {
            Type tType = inputObj.GetType();
            Attribute attr = tType.GetCustomAttribute(typeof(SerializableAttribute));
            if (attr == null) throw new ShipengConvertException("未标识为可序列化");
            using (var stream = new MemoryStream())
            {
                var bf2 = new BinaryFormatter();
                bf2.Serialize(stream, inputObj);
                stream.Position = 0;
                return (T)bf2.Deserialize(stream);
            }
        }
        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns>克隆的对象</returns>
        public static T Clone<T>(this T inputObj)
        {
            Type tType = inputObj.GetType();
            Attribute attr = tType.GetCustomAttribute(typeof(SerializableAttribute));
            return attr != null ? CloneBySerializable(inputObj) : CloneByReflex(inputObj);
        }

    }
}