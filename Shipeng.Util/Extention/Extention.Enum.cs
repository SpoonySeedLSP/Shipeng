using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
    }
}
