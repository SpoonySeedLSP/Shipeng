using System;
using System.Collections.Generic;
using System.Reflection;

namespace Shipeng.Mapster
{
    /// <summary>
    /// 自定义模型映射
    /// </summary>
    public static class MapperTool
    {
        /// <summary>
        /// 反射实现两个类的对象之间相同属性的值的复制
        /// 适用于初始化新实体(适用于建立实体的时候从一个实体做为数据源赋值app)
        /// </summary>
        /// <typeparam name="D">返回的实体</typeparam>
        /// <typeparam name="S">数据源实体</typeparam>
        /// <param name="s">数据源实体</param>
        /// <returns>返回的新实体</returns>
        public static D Mapper<D, S>(this S s)
        {
            D d = Activator.CreateInstance<D>(); //构造新实例
            try
            {
                var Types = s.GetType();//得到类型  
                var Typed = typeof(D);
                foreach (PropertyInfo sp in Types.GetProperties())//得到类型的属性字段  
                {
                    foreach (PropertyInfo dp in Typed.GetProperties())
                    {
                        if (dp.Name.ToLower() == sp.Name.ToLower() && dp.PropertyType == sp.PropertyType && dp.Name != "Error" && dp.Name != "Item")//判断属性名是否相同  
                        {
                            dp.SetValue(d, sp.GetValue(s, null), null);//得到s对象属性的值复制给d对象的属性  
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return d;
        }

        /// <summary>
        /// 反射实现两个类的对象之间相同属性的值的复制
        /// 适用于初始化新实体(适用于建立实体的时候从一个实体做为数据源赋值app)
        /// </summary>
        /// <typeparam name="D">返回的实体</typeparam>
        /// <typeparam name="S">数据源实体</typeparam>
        /// <param name="s">数据源实体</param>
        /// <param name="d"></param>
        /// <returns>返回的新实体</returns>
        public static D Mapper<D, S>(this S s,D d)
        {
            try
            {
                var Types = s.GetType();//得到类型  
                var Typed = typeof(D);
                foreach (PropertyInfo sp in Types.GetProperties())//得到类型的属性字段  
                {
                    foreach (PropertyInfo dp in Typed.GetProperties())
                    {
                        if (dp.Name.ToLower() == sp.Name.ToLower() && dp.PropertyType == sp.PropertyType && dp.Name != "Error" && dp.Name != "Item")//判断属性名是否相同  
                        {
                            dp.SetValue(d, sp.GetValue(s, null), null);//得到s对象属性的值复制给d对象的属性  
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return d;
        }

        /// <summary>
        /// 对象集合转换
        /// </summary>
        /// <typeparam name="T">目标实体</typeparam>
        /// <typeparam name="S">数据源实体</typeparam>
        /// <param name="source">源数据</param>
        /// <returns></returns>
        public static List<T> ListToList<T, S>(this List<S> source)
        {
            List<T> list = new List<T>();
            foreach (S data in source)
            {
                T obj = Mapper<T, S>(data);
                list.Add(obj);
            }
            return list;
        }

    }
}
