
namespace Shipeng.Util.Helper
{
    public static partial class ChinaIDHelper
    {
        /// <summary>
        /// 验证身份证号码
        /// </summary>
        /// <param name="Id">身份证号码</param>
        /// <returns>验证成功为True，否则为False</returns>
        public static bool CheckIDCard(string Id)
        {
            if (Id.Length == 18)
            {
                bool check = CheckIDCard18(Id);
                return check;
            }
            else if (Id.Length == 15)
            {
                bool check = CheckIDCard15(Id);
                return check;
            }
            else
            {
                return false;
            }
        }

        #region 身份证号码验证

        /// <summary>
        /// 验证15位身份证号
        /// </summary>
        /// <param name="Id">身份证号</param>
        /// <returns>验证成功为True，否则为False</returns>
        private static bool CheckIDCard18(string Id)
        {
            if (long.TryParse(Id.Remove(17), out long n) == false || n < Math.Pow(10, 16) || long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = Id.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }
            return true;//符合GB11643-1999标准
        }

        /// <summary>
        /// 验证18位身份证号
        /// </summary>
        /// <param name="Id">身份证号</param>
        /// <returns>验证成功为True，否则为False</returns>
        private static bool CheckIDCard15(string Id)
        {
            if (long.TryParse(Id, out long n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            return true;//符合15位身份证标准
        }

        #endregion 身份证号码验证

        /// <summary>
        /// 身份证获取性别
        /// </summary>
        /// <param name="IdCard"></param>
        /// <returns></returns>
        public static Sex GetSexCheckID(this string IdCard)
        {
            try
            {
                string tmp = "";
                if (IdCard.Length == 15)
                {
                    tmp = IdCard.Substring(IdCard.Length - 3);
                }
                else if (IdCard.Length == 18)
                {
                    tmp = IdCard.Substring(IdCard.Length - 4);
                    tmp = tmp.Substring(0, 3);
                }
                if (string.IsNullOrEmpty(tmp))
                {
                    return Sex.Unknown;
                }

                int sx = int.Parse(tmp);
                Math.DivRem(sx, 2, out int outNum);
                if (outNum == 0)
                {
                    return Sex.Woman;
                }
                else
                {
                    return Sex.Man;
                }
            }
            catch (Exception)
            {
                return Sex.Unknown;
            }
        }

        /// <summary>
        /// 身份证获取出生日期
        /// </summary>
        /// <param name="identityCard"></param>
        /// <returns></returns>
        public static string GetBirthdayCheckID(this string identityCard)
        {
            string Birthday = "";
            if (identityCard.Length == 18)//处理18位的身份证号码从号码中得到生日和性别代码
            {
                Birthday = identityCard.Substring(6, 4) + "-" + identityCard.Substring(10, 2) + "-" + identityCard.Substring(12, 2);
            }
            if (identityCard.Length == 15)
            {
                Birthday = "19" + identityCard.Substring(6, 2) + "-" + identityCard.Substring(8, 2) + "-" + identityCard.Substring(10, 2);
            }
            return Birthday;
        }

        /// <summary>
        /// 身份证获取年龄
        /// </summary>
        /// <param name="identityCard"></param>
        /// <returns></returns>
        public static int GetAgeCheckID(this string identityCard)
        {
            int age = 0;
            string birthDay = identityCard.GetBirthdayCheckID();
            if (!birthDay.IsNullOrEmpty())
            {
                DateTime birthDate = DateTime.Parse(birthDay);
                DateTime nowDateTime = DateTime.Now;
                age = nowDateTime.Year - birthDate.Year;
                //再考虑月、天的因素
                if (nowDateTime.Month < birthDate.Month || (nowDateTime.Month == birthDate.Month && nowDateTime.Day < birthDate.Day))
                {
                    age--;
                }
            }
            return age;
        }

        /// <summary>
        /// 省
        /// </summary>
        /// <param name="identityCard"></param>
        /// <returns></returns>
        public static string GetProvinceCheckID(this string identityCard)
        {
            return "";
        }

        /// <summary>
        ///市
        /// </summary>
        /// <param name="identityCard"></param>
        /// <returns></returns>
        public static string GetCityCheckID(this string identityCard)
        {
            return "";
        }

        /// <summary>
        /// 区
        /// </summary>
        /// <param name="identityCard"></param>
        /// <returns></returns>
        public static string GetAreaCheckID(this string identityCard)
        {
            return "";
        }
    }
}