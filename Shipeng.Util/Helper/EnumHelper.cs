using System;
using System.Collections.Generic;
using System.Linq;

namespace Shipeng.Util
{
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
        /// 多选枚举转为对应文本,逗号隔开
        /// </summary>
        /// <param name="values"> 多个值 </param>
        /// <param name="enumType"> 枚举类型 </param>
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
    }
}
