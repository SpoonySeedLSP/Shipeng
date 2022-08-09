using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Shipeng.Util
{
    public static partial class Extention
    {
        private static BindingFlags _bindingFlags { get; }
            = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

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

    }
}