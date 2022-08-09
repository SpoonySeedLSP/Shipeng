using System;
using System.ComponentModel;
using System.Reflection;

namespace Shipeng.Util
{
    public static partial class Extention
    {
        /// <summary>
        /// 获取属性描述
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetDesc(this PropertyInfo t)
        {
            string des = ((DescriptionAttribute)Attribute.GetCustomAttribute(t, typeof(DescriptionAttribute)))?.Description ?? null;// 属性描述
            return des;
        }
    }
}
