using System.Text;

namespace Shipeng.Util
{
    /// <summary>
    /// 生成指定长度的随机字符串
    /// </summary>
    public class GetSteingRandomizerHelper
    {
        #region 生成指定长度的随机字符串

        /// <summary>
        /// 生成指定长度的随机字符串
        /// </summary>
        /// <param name="intLength"> 随机字符串长度 </param>
        /// <param name="booNumber"> 生成的字符串中是否包含数字 </param>
        /// <param name="booSign"> 生成的字符串中是否包含符号 </param>
        /// <param name="booSmallword"> 生成的字符串中是否包含小写字母 </param>
        /// <param name="booBigword"> 生成的字符串中是否包含大写字母 </param>
        /// <returns> </returns>
        public static string GetRandomizer(int intLength = 6, bool booNumber = true, bool booSign = false, bool booSmallword = true, bool booBigword = true)
        {
            //定义
            Random ranA = new Random();
            int intResultRound = 0;
            int intA = 0;
            string strB = "";

            while (intResultRound < intLength)
            {
                //生成随机数A，表示生成类型
                //1=数字，2=符号，3=小写字母，4=大写字母

                intA = ranA.Next(1, 5);
                switch (intA)
                {
                    case 1://数字
                        if (booNumber)
                        {
                            intA = ranA.Next(0, 10);
                            strB += intA.ToString();
                            intResultRound++;
                        }
                        break;

                    case 2://符号
                        if (booSign)
                        {
                            intA = ranA.Next(33, 48);
                            strB += ((char)intA).ToString();
                            intResultRound++;
                        }
                        break;

                    case 3://小写字母
                        if (booSmallword)
                        {
                            intA = ranA.Next(97, 123);
                            strB += ((char)intA).ToString();
                            intResultRound++;
                        }
                        break;

                    case 4://大写字母
                        if (booBigword)
                        {
                            intA = ranA.Next(65, 89);
                            strB += ((char)intA).ToString();
                            intResultRound++;
                        }
                        break;

                    default:
                        intA = ranA.Next(0, 10);
                        strB += intA.ToString();
                        intResultRound++;
                        break;
                }
                continue;
            }
            return strB;
        }

        #endregion 生成指定长度的随机字符串

        #region 生成制定位数的随机码（数字）

        /// <summary>
        ///生成制定位数的随机码（数字）
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateRandomCode(int length)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                Random r = new Random(Guid.NewGuid().GetHashCode());
                result.Append(r.Next(0, 10));
            }
            return result.ToString();
        }

        #endregion 生成制定位数的随机码（数字）
    }
}