using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;

namespace Shipeng.Util
{
    /// <summary>
    /// 枚举工具类
    /// Author:李仕鹏
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 将枚举类型转为选项列表 注：value为值,text为显示内容
        /// </summary>
        /// <param name="enumType"> 枚举类型 </param>
        /// <returns> </returns>
        public static List<SelectOption> ToOptionList(Type enumType)
        {
            Array values = System.Enum.GetValues(enumType);
            List<SelectOption> list = new List<SelectOption>();
            foreach (object aValue in values)
            {
                list.Add(new SelectOption
                {
                    value = ((int)aValue).ToString(),
                    text = aValue.ToString()
                });
            }

            return list;
        }

        /// <summary>
        /// 根据枚举类型得到其所有的 值 与 枚举定义Description属性 的集合  
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <returns></returns>
        public static NameValueCollection GetNVCFromEnumValue(Type enumType)
        {
            try
            {
                NameValueCollection nvc = new NameValueCollection();
                Type typeDescription = typeof(DescriptionAttribute);
                FieldInfo[] fields = enumType.GetFields();
                string strText = string.Empty;
                string strValue = string.Empty;
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType.IsEnum)
                    {
                        strValue = ((int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null)).ToString();
                        object[] arr = field.GetCustomAttributes(typeDescription, true);
                        if (arr.Length > 0)
                        {
                            DescriptionAttribute aa = (DescriptionAttribute)arr[0];
                            strText = aa.Description;
                        }
                        else
                        {
                            strText = "";
                        }
                        nvc.Add(strValue, strText);
                    }
                }
                return nvc;
            }
            catch (Exception ex)
            {
                throw new Exception($"根据枚举类型得到其所有的值与枚举定义Description属性的集合发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 根据枚举值得到属性Description中的描述, 如果没有定义此属性则返回空串 
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <param name="enumType">枚举类型</param>
        /// <returns></returns>
        public static string GetEnumDescriptionString(int value, Type enumType)
        {
            try
            {
                NameValueCollection nvc = GetNVCFromEnumValue(enumType);
                return nvc[value.ToString()];
            }
            catch (Exception ex)
            {
                throw new Exception($"根据枚举值得到属性Description中的描述发生异常：{ex.Message}");
            }
        }       

        /// <summary>
        /// 多选枚举转为对应文本,逗号隔开
        /// </summary>
        /// <param name="values">多个值</param>
        /// <param name="enumType">枚举类型</param>
        /// <returns> </returns>
        public static string ToMultipleText(List<int> values, Type enumType)
        {
            if (values == null)
            {
                return string.Empty;
            }

            List<string> textList = new List<string>();

            Array allValues = System.Enum.GetValues(enumType);
            foreach (object aValue in allValues)
            {
                if (values.Contains((int)aValue))
                {
                    textList.Add(aValue.ToString());
                }
            }

            return string.Join(",", textList);
        }

        /// <summary>
        /// 多选枚举转为对应文本,逗号隔开
        /// </summary>
        /// <param name="values"> 多个值逗号隔开 </param>
        /// <param name="enumType"> 枚举类型 </param>
        /// <returns> </returns>
        public static string ToMultipleText(string values, Type enumType)
        {
            return ToMultipleText(values?.Split(',')?.Select(x => x.ToInt())?.ToList(), enumType);
        }

        /// <summary>
        /// 获取枚举
        /// </summary>
        /// <param name="Name"> 枚举名称 </param>
        /// <returns> </returns>
        public static List<EnumEitityDTO> GetGetEnumList(string Name)
        {
            Type s = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(u => u.Name == Name).FirstOrDefault();

            if (s.IsNullOrEmpty() || !s.IsEnum)
            {
                throw new BusException("非有效枚举!");
            }
            Array values = System.Enum.GetValues(s);

            List<EnumEitityDTO> list = new List<EnumEitityDTO>();
            foreach (object aValue in values)
            {
                list.Add(new EnumEitityDTO
                {
                    Id = Convert.ToInt32(aValue),
                    Describe = s.GetDescription(aValue.ToString()),
                    EnumName = aValue.ToString()
                });
            }
            return list;
        }

        /// <summary>
        /// 结果为负数已开始/已结束
        /// 结果是正数未开始/未结束
        /// </summary>
        /// <param name="timeA"></param>
        /// <returns></returns>
        public static int GetTotalSecondsTime(DateTime timeA)
        {
            //timeA 表示需要计算
            DateTime timeB = DateTime.Now;	//获取当前时间
            TimeSpan ts = timeA - timeB;	//计算时间差
            int time = (int)ts.TotalSeconds;	//将时间差转换为秒
            return time;
        }

        /// <summary>
        /// 结果为负数已开始/已结束
        /// 结果是正数未开始/未结束
        /// </summary>
        /// <param name="timeA"></param>
        /// <returns></returns>
        public static int GetTotalMinutesTime(DateTime timeA)
        {
            //timeA 表示需要计算
            DateTime timeB = DateTime.Now;	//获取当前时间
            TimeSpan ts = timeA - timeB;	//计算时间差
            int time = (int)ts.TotalMinutes;	//将时间差转换为分钟
            return time;
        }

        /// <summary>
        /// 结果为负数已开始/已结束
        /// 结果是正数未开始/未结束
        /// </summary>
        /// <param name="timeA"></param>
        /// <returns></returns>
        public static int GetTotalHoursTime(DateTime timeA)
        {
            //timeA 表示需要计算
            DateTime timeB = DateTime.Now;	//获取当前时间
            TimeSpan ts = timeA - timeB;	//计算时间差
            int time = (int)ts.TotalHours;	//将时间差转换为小时
            return time;
        }

        /// <summary>
        /// 获取枚举列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDic<T>()
        {
            Dictionary<int, string> list = new Dictionary<int, string>();
            foreach (var e in System.Enum.GetValues(typeof(T)))
            {
                list.Add(Convert.ToInt32(e), e.GetDescriptionByEnum<T>());
            }
            return list;
        }

        /// <summary>
        /// 获取枚举列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> EnumToList<T>()
        {
            List<string> list = new List<string>();

            foreach (var e in System.Enum.GetValues(typeof(T)))
            {
                list.Add(e.GetDescriptionByEnum<T>());
            }
            return list;
        }

        /// <summary>
        /// 获取枚举项列表
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="enumObj">枚举项</param>
        /// <param name="markCurrentAsSelected">是否选中当前枚举项</param>
        /// <param name="valuesToExclude">不包含的枚举项</param>
        /// <returns>通用枚举列表</returns>
        public static IList<EnumNode> ToSelectList<TEnum>(this TEnum enumObj,
           bool markCurrentAsSelected = true, int[] valuesToExclude = null) where TEnum : struct
        {
            IList<EnumNode> values = new List<EnumNode>();
            if (!typeof(TEnum).IsEnum) throw new ArgumentException("枚举类型是必需的", "enumObj");
            var enums = from TEnum enumValue in System.Enum.GetValues(typeof(TEnum))
                        where valuesToExclude == null || !valuesToExclude.Contains(Convert.ToInt32(enumValue))
                        select enumValue;
            foreach (var enumValue in enums)
            {
                var value = new EnumNode
                {
                    Id = Convert.ToInt32(enumValue),
                    Value = enumValue.ToString(),
                    Name = enumValue.ToString()
                };
                FieldInfo field = typeof(TEnum).GetField(enumValue.ToString());
                if (field != null)
                {
                    var attrs = field.GetCustomAttributes();
                    foreach (var attr in attrs)
                    {
                        var propertyName = attr.GetType().GetProperty("Name");
                        if (propertyName != null)
                        {
                            value.Name = propertyName.GetValue(attr).ToString();
                        }
                    }
                }
                if (markCurrentAsSelected && Convert.ToInt32(enumObj) == value.Id)
                {
                    value.Selected = true;
                }
                values.Add(value);
            }
            return values;
        }

    }


    /// <summary>
    /// 枚举通用类
    /// </summary>
    public class EnumNode
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 文本值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Selected { get; set; }
    }

}
