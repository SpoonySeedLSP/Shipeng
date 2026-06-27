using System.Reflection;

namespace Shipeng.Util.Helper
{
    /// <summary>
    /// 比较工具类
    /// </summary>
    public class CompareHelper
    {
        /// <summary>
        /// 判断两个相同引用类型的对象的属性值是否相等
        /// 深度比较
        /// 比较两个对象所有属性值是否相等，包括属性值的属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj1">对象1</param>
        /// <param name="obj2">对象2</param>
        /// <param name="type">按type类型中的属性进行比较</param>
        /// <returns></returns>
        public static bool CompareProperties<T>(T obj1, T obj2, Type type)
        {
            //为空判断
            if (obj1 == null && obj2 == null)
                return true;
            else if (obj1 == null || obj2 == null)
                return false;
            Type t = type;
            PropertyInfo[] props = t.GetProperties();
            foreach (var po in props)
            {
                if (IsCanCompare(po.PropertyType))
                {
                    if (!po.GetValue(obj1).Equals(po.GetValue(obj2)))
                    {
                        return false;
                    }
                }
                else
                {
                    var b = CompareProperties(po.GetValue(obj1), po.GetValue(obj2), po.PropertyType);
                    if (!b) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 该类型是否可直接进行值的比较
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool IsCanCompare(Type t)
        {
            if (t.IsValueType)
            {
                return true;
            }
            else
            {
                //String是特殊的引用类型，它可以直接进行值的比较
                if (t.FullName == typeof(string).FullName)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 比较2个字符串的相似度（使用余弦相似度）
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns>0-1之间的数</returns>
        public static double SimilarityCos(string str1, string str2)
        {
            str1 = str1.Trim();
            str2 = str2.Trim();
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;
            List<string> lstr1 = SimpParticiple(str1);
            List<string> lstr2 = SimpParticiple(str2);
            //求并集
            var strUnion = lstr1.Union(lstr2);
            //求向量
            List<int> int1 = new List<int>();
            List<int> int2 = new List<int>();
            foreach (var item in strUnion)
            {
                int1.Add(lstr1.Count(o => o == item));
                int2.Add(lstr2.Count(o => o == item));
            }
            double s = 0;
            double den1 = 0;
            double den2 = 0;
            for (int i = 0; i < int1.Count(); i++)
            {
                //求分子
                s += int1[i] * int2[i];
                //求分母（1）
                den1 += Math.Pow(int1[i], 2);
                //求分母（2）
                den2 += Math.Pow(int2[i], 2);
            }
            return s / (Math.Sqrt(den1) * Math.Sqrt(den2));
        }

        /// <summary>
        /// 简单分词（需要更好的效果，需要这里优化，比如把：【今天天气很好】，分成【今天，天气，很好】，同时可以做同义词优化，【今天】=【今日】效果更好）
        /// </summary>
        public static List<string> SimpParticiple(string str)
        {
            List<string> vs = new List<string>();
            foreach (var item in str)
            {
                vs.Add(item.ToString());
            }
            return vs;
        }

        /// <summary>
        /// 比较两个日期大小
        /// 日期一是否大于日期二
        /// </summary>
        /// <param name="dateStr1">日期1</param>
        /// <param name="dateStr2">日期2</param>
        /// <returns>true.日期1大于等于日期2</returns>
        public static bool CompanyDate(DateTime dateStr1, DateTime dateStr2)
        {
            // 将日期字符串转换为日期对象
            int compNum = DateTime.Compare(dateStr1, dateStr2);
            // dateStr1> dateStr2
            if (compNum > 0) return true;
            // dateStr1= dateStr2
            if (compNum == 0) return true;
            // dateStr1< dateStr2
            if (compNum < 0) return false;
            return false;
        }
    }
}
