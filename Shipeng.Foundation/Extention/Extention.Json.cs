using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Shipeng.Util
{
    /// <summary>
    /// 拓展类
    /// </summary>
    public static class JsonExtention
    {
        static JsonExtention()
        {
            JsonConvert.DefaultSettings = () => DefaultJsonSetting;
        }

        public static JsonSerializerSettings DefaultJsonSetting = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            DateFormatString = "yyyy-MM-dd HH:mm:ss.fff"
        };

        /// <summary>
        /// 把数组转为逗号连接的字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Str"></param>
        /// <returns></returns>
        public static string ArrayToString(dynamic data, string Str)
        {
            string resStr = Str;
            foreach (var item in data)
            {
                if (resStr != "")
                {
                    resStr += ",";
                }

                if (item is string)
                {
                    resStr += item;
                }
                else
                {
                    resStr += item.Value;

                }
            }
            return resStr;
        }

        /// <summary>
        /// 将对象序列化成Json字符串
        /// </summary>
        /// <param name="obj"> 需要序列化的对象 </param>
        /// <returns> </returns>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static string ToJson(this object obj, string dateFormat = "yyyy/MM/dd HH:mm:ss")
        {
            return obj == null ? string.Empty : JsonConvert.SerializeObject(obj, new IsoDateTimeConverter { DateTimeFormat = dateFormat });
        }

        public static object ToObject(this string Json)
        {
            return string.IsNullOrEmpty(Json) ? null : JsonConvert.DeserializeObject(Json);
        }

        /// <summary>
        /// 将Json字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T"> 对象类型 </typeparam>
        /// <param name="jsonStr"> Json字符串 </param>
        /// <returns> </returns>
        public static T ToObject<T>(this string jsonStr)
        {
            jsonStr = jsonStr.Replace("&nbsp;", "");
            return jsonStr == null ? default(T) : JsonConvert.DeserializeObject<T>(jsonStr);
        }

        /// <summary>
        /// 将Json字符串反序列化为对象
        /// </summary>
        /// <param name="jsonStr"> json字符串 </param>
        /// <param name="type"> 对象类型 </param>
        /// <returns> </returns>
        public static object ToObject(this string jsonStr, Type type)
        {
            return JsonConvert.DeserializeObject(jsonStr, type);
        }
    }
}
