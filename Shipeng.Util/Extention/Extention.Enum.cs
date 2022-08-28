using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

namespace Shipeng.Util
{
    /// <summary>
    /// 拓展类
    /// </summary>
    public static partial class Extention
    {
        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="value"> 枚举值 </param>
        /// <returns> </returns>
        public static string GetDescription(this System.Enum value)
        {
            if (value.IsNullOrEmpty())
            {
                return null;
            }
            DescriptionAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        /// <summary>
        /// 知道枚举,传入枚举英文,获取描述
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="Name"> </param>
        /// <returns> </returns>
        public static string GetDescription(this Type value, string Name)
        {
            DescriptionAttribute attribute = value.GetField(Name)
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        /// <summary>
        /// 获取实体层描述
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="Name"> </param>
        /// <returns> </returns>
        public static string GetEntityDescription(this Type value, string Name)
        {
            PropertyDescriptor s = TypeDescriptor.GetProperties(value)[Name];
            DescriptionAttribute description = s == null ? null : s.Attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;
            return description == null ? "" : description.Description;
        }

        /// <summary>
        /// 获取实体层描述
        /// </summary>
        /// <param name="s"> </param>
        /// <returns> </returns>
        public static string GetEntityDescription(this PropertyDescriptor s)
        {
            DescriptionAttribute description = s == null ? null : s.Attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;
            return description == null ? "" : description.Description;
        }

        /// <summary>
        /// 获取类的所有属性
        /// </summary>
        /// <param name="source"> 是否所有属性名称均为小写 </param>
        /// <param name="IsLower"> 是否所有属性名称均为小写 </param>
        /// <returns> 返回属性名称数组 </returns>
        public static string[] ToTypeArray(this object source, bool IsLower = true)
        {
            if (source == null)
            {
                return new string[] { };
            }

            List<string> arraylist = new List<string>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                if (IsLower)
                {
                    arraylist.Add(property.Name.ToLower());
                }
                else
                {
                    arraylist.Add(property.Name);
                }
            }
            return arraylist.ToArray();
        }

        /// <summary>
        /// 获取类的所有属性
        /// </summary>
        /// <param name="source"> 是否所有属性名称均为小写 </param>
        /// <returns> 返回属性名称数组 </returns>
        public static List<string> ToTypeList(this object source)
        {
            List<string> arraylist = new List<string>();
            if (source == null)
            {
                return arraylist;
            }
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                arraylist.Add(property.Name);
            }
            return arraylist;
        }

        /// <summary>
        /// 类所有属性转化成带分隔符的字符串
        /// </summary>
        /// <param name="source"> 类 </param>
        /// <param name="IsLower"> 是否小写 </param>
        /// <param name="Segmenter"> 分割符 </param>
        /// <returns> 返回字符串 </returns>
        public static string ToStringJoin(this object source, bool IsLower = true, char Segmenter = ',')
        {
            string[] str = source.ToTypeArray(IsLower);
            return string.Join(Segmenter, str);
        }

        #region 枚举成员转成dictionary类型
        /// <summary>
        /// 转成dictionary类型
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDictionary(this Type enumType)
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            Type typeDescription = typeof(DescriptionAttribute);
            FieldInfo[] fields = enumType.GetFields();
            int sValue = 0;
            string sText = string.Empty;
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    sValue = ((int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null));
                    object[] arr = field.GetCustomAttributes(typeDescription, true);
                    if (arr.Length > 0)
                    {
                        DescriptionAttribute da = (DescriptionAttribute)arr[0];
                        sText = da.Description;
                    }
                    else
                    {
                        sText = field.Name;
                    }
                    dictionary.Add(sValue, sText);
                }
            }
            return dictionary;
        }
        /// <summary>
        /// 枚举成员转成键值对Json字符串
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string EnumToDictionaryString(this Type enumType)
        {
            List<KeyValuePair<int, string>> dictionaryList = EnumToDictionary(enumType).ToList();
            var sJson = JsonConvert.SerializeObject(dictionaryList);
            return sJson;
        }
        #endregion

        #region 获取枚举的描述
        ///// <summary>
        ///// 获取枚举值对应的描述
        ///// </summary>
        ///// <param name="enumType"></param>
        ///// <returns></returns>
        //public static string GetDescription(this System.Enum enumType)
        //{
        //    FieldInfo EnumInfo = enumType.GetType().GetField(enumType.ToString());
        //    if (EnumInfo != null)
        //    {
        //        DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])EnumInfo
        //            .GetCustomAttributes(typeof(DescriptionAttribute), false);
        //        if (EnumAttributes.Length > 0)
        //        {
        //            return EnumAttributes[0].Description;
        //        }
        //    }
        //    return enumType.ToString();
        //}
        #endregion

        #region 根据值获取枚举的描述
        public static string GetDescriptionByEnum<T>(this object obj)
        {
            var tEnum = System.Enum.Parse(typeof(T), obj.ParseToString()) as System.Enum;
            var description = tEnum.GetDescription();
            return description;
        }
        /// <summary>
        /// 枚举
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string ToDescription(this System.Enum enumType)
        {
            if (enumType == null)
                return "";

            System.Reflection.FieldInfo fieldInfo = enumType.GetType().GetField(enumType.ToString());

            object[] attribArray = fieldInfo.GetCustomAttributes(false);
            if (attribArray.Length == 0)
                return enumType.ToString();
            else
                return (attribArray[0] as DescriptionAttribute).Description;
        }
        #endregion


    }
}
