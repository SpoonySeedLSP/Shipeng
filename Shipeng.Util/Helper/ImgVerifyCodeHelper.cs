using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Shipeng.Util
{
    public static class ImgVerifyCodeHelper
    {
        /// <summary>
        /// 生成图片验证码
        /// </summary>
        /// <param name="length"> 验证码字符数 </param>
        /// <returns> 图片byte[]和code </returns>
        public static (byte[] imgBytes, string code) BuildVerifyCode(int length)
        {
            VerifyCodeFactory vc = new VerifyCodeFactory();
            int l = new Random().Next(1, 100);

            if (l % 2 == 0)
            {
                int width = 100;
                int height = 40;
                int fontsize = 20;
                string code = string.Empty;
                byte[] bytes = vc.W_CreateValidateGraphic(out code, length, width, height, fontsize);

                return (bytes, code);
            }
            else
            {
                (string Calculate, string Result) = vc.CalculationVerificationCode(0);
                byte[] bytes = vc.CreateValidateGraphic(Calculate);
                return (bytes, Result);
            }
        }

        /// <summary>
        /// 生成图片验证码
        /// </summary>
        /// <param name="length"> 验证码字符数 </param>
        /// <returns> 图片byte[]和code </returns>
        public static (byte[] imgBytes, string code) BuildVerifyCodeIng(int length)
        {
            VerifyCodeFactory vc = new VerifyCodeFactory();
            string code = vc.CreateValidateCode(0, length);
            byte[] bytes = vc.CreateValidateGraphic(code);

            return (bytes, code);
        }

        /// <summary>
        /// 加减乘除计算验证码
        /// </summary>
        /// <returns></returns>
        public static (byte[] imgBytes, string code) BuildVerifyCodeIng()
        {
            VerifyCodeFactory vc = new VerifyCodeFactory();
            (string Calculate, string Result) = vc.CalculationVerificationCode(0);
            byte[] bytes = vc.CreateValidateGraphic(Calculate);

            return (bytes, Result);
        }

        public class VerifyCodeFactory
        {
            /// <summary>
            /// 验证码的字符集，去掉了一些容易混淆的字符
            /// </summary>
            //private char[] character = { '2', '3', '4', '5', '6', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };

            private readonly char[] character = { '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'm', 'n', 'p', 'r', 's', 't', 'w', 'x', 'y', 'z' };

            /// <summary>
            /// 得到验证码的高度
            /// </summary>
            /// <returns> </returns>
            public static double GetImageHeight()
            {
                return 22.5;
            }

            /// <summary>
            /// 得到验证码图片的长度
            /// </summary>
            /// <param name="validateNumLength"> 验证码的长度 </param>
            /// <returns> </returns>
            public static int GetImageWidth(int validateNumLength)
            {
                return (int)(validateNumLength * 12.0);
            }

            /// <summary>
            /// ///
            /// </summary>
            /// <param name="codeType"> 验证码类型(0-字母数字混合,1-数字,2-字母) </param>
            /// <param name="codeCount"> 验证码字符个数 </param>
            /// <returns> </returns>
            public string CreateValidateCode(int codeType, int codeCount)
            {
                char code;
                string checkCode = string.Empty;
                System.Random random = new Random();

                for (int i = 0; i < codeCount; i++)
                {
                    code = character[random.Next(character.Length)];

                    // 要求全为数字或字母
                    if (codeType == 1)
                    {
                        if (code < 48 || code > 57)
                        {
                            i--;
                            continue;
                        }
                    }
                    else if (codeType == 2)
                    {
                        if (code < 65 || code > 90)
                        {
                            i--;
                            continue;
                        }
                    }
                    checkCode += code;
                }

                return checkCode;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="i">0:随机  1:+ 2:- 3:x 4:/</param>
            /// <returns></returns>
            public (string Calculate, string Result) CalculationVerificationCode(int i)
            {
                int A = 0; int B = 0; int C = 0;

                string CalculateTxt = "";
                if (i == 0)
                {
                    i = GetRandom(1, 5);
                }

                switch (i)
                {
                    //“+”是加号，“-”是减号，“×”是乘号，“÷”是除号。
                    case 1:
                        A = GetRandom(0, 10);
                        B = GetRandom(1, 12);
                        CalculateTxt = A + "+" + B + "=?";
                        C = A + B;
                        break;

                    case 2:
                        A = GetRandom(8, 20);
                        B = GetRandom(0, A);
                        CalculateTxt = A + "-" + B + "=?";
                        C = A - B;
                        break;

                    case 3:
                        A = GetRandom(0, 11);
                        B = GetRandom(0, 11);
                        CalculateTxt = A + "×" + B + "=?";
                        C = A * B;
                        break;

                    case 4:
                        B = GetRandom(1, 10);
                        C = GetRandom(1, 10);
                        A = B * C;
                        CalculateTxt = A + "÷" + B + "=?";
                        break;

                    default:
                        B = GetRandom(1, 10);
                        C = GetRandom(3, 10);
                        A = B * C;
                        CalculateTxt = A + "÷" + B + "=?";
                        break;
                }

                return (CalculateTxt, C.ToString());
            }

            /// <summary>
            /// 获取随机数的访问
            /// </summary>
            /// <param name="minValue"></param>
            /// <param name="maxValue"></param>
            /// <returns></returns>
            public static int GetRandom(int minValue, int maxValue)
            {
                return new Random().Next(minValue, maxValue);
            }

            /// <summary>
            /// 生成验证码
            /// </summary>
            /// <param name="length"> 指定验证码的长度 </param>
            /// <returns> </returns>
            public string CreateValidateCode(int length)
            {
                int[] randMembers = new int[length];
                int[] validateNums = new int[length];
                string validateNumberStr = "";
                //生成起始序列值
                int seekSeek = unchecked((int)DateTime.Now.Ticks);
                Random seekRand = new Random(seekSeek);
                int beginSeek = seekRand.Next(0, int.MaxValue - length * 10000);
                int[] seeks = new int[length];
                for (int i = 0; i < length; i++)
                {
                    beginSeek += 10000;
                    seeks[i] = beginSeek;
                }
                //生成随机数字
                for (int i = 0; i < length; i++)
                {
                    Random rand = new Random(seeks[i]);
                    int pownum = 1 * (int)Math.Pow(10, length);
                    randMembers[i] = rand.Next(pownum, int.MaxValue);
                }
                //抽取随机数字
                for (int i = 0; i < length; i++)
                {
                    string numStr = randMembers[i].ToString();
                    int numLength = numStr.Length;
                    Random rand = new Random();
                    int numPosition = rand.Next(0, numLength - 1);
                    validateNums[i] = int.Parse(numStr.Substring(numPosition, 1));
                }
                //生成验证码
                for (int i = 0; i < length; i++)
                {
                    validateNumberStr += validateNums[i].ToString();
                }
                return validateNumberStr;
            }

            public byte[] CreateValidateGraphic(string validateCode)
            {
                return CreateValidateGraphic(validateCode, 14, 24);
            }

            /// <summary>
            /// 创建验证码的图片
            /// </summary>
            /// <param name="validateCode"> 验证码 </param>
            /// <param name="fontsize"> </param>
            /// <param name="height"> </param>
            public byte[] CreateValidateGraphic(string validateCode, float fontsize, int height = 40)
            {
                Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * (fontsize + 1)), height);
                Graphics g = Graphics.FromImage(image);
                try
                {
                    //生成随机生成器
                    Random random = new Random();
                    //清空图片背景色
                    g.Clear(Color.White);

                    Color[] oColors = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.DarkBlue };
                    FontStyle[] Font = { FontStyle.Bold, FontStyle.Italic, FontStyle.Regular, FontStyle.Underline, FontStyle.Strikeout };
                    //字体列表，用于验证码
                    string[] oFontNames = { "Times New Roman", "MS Mincho", "Book Antiqua", "Gungsuh", "PMingLiU", "Impact" };
                    Random oRnd = new Random();

                    if (!validateCode.Contains("?"))
                    {
                        //画图片的干扰线
                        for (int i = 0; i < 7; i++)
                        {
                            int x1 = random.Next(image.Width);
                            int x2 = random.Next(image.Width);
                            int y1 = random.Next(image.Height);
                            int y2 = random.Next(image.Height);
                            g.DrawLine(new Pen(oColors[oRnd.Next(oColors.Length)]), x1, y1, x2, y2);
                        }
                    }

                    //画图片验证码
                    string sFontName = oFontNames[oRnd.Next(oFontNames.Length)];

                    Rectangle layoutRectange = new Rectangle(0, 0, image.Width, image.Height);
                    Font font = new Font(sFontName, fontsize, Font[oRnd.Next(Font.Length)]);
                    LinearGradientBrush brush = new LinearGradientBrush(layoutRectange, oColors[oRnd.Next(oFontNames.Length)], oColors[oRnd.Next(oFontNames.Length)], 1.2f, true);
                    StringFormat format = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(validateCode, font, brush, layoutRectange, format);

                    //画图片的前景干扰点
                    for (int i = 0; i < 60; i++)
                    {
                        int x = random.Next(image.Width);
                        int y = random.Next(image.Height);
                        image.SetPixel(x, y, oColors[oRnd.Next(oColors.Length)]);
                    }

                    //画图片的边框线
                    //g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                    //保存图片数据
                    MemoryStream stream = new MemoryStream();
                    image.Save(stream, ImageFormat.Jpeg);
                    //输出图片流
                    return stream.ToArray();
                }
                finally
                {
                    g.Dispose();
                    image.Dispose();
                }
            }

            /// <summary>
            /// 生成简洁漂亮的图形验证码
            /// </summary>
            /// <param name="Code"> 传出验证码 </param>
            /// <param name="CodeLength"> 验证码长度 </param>
            /// <param name="Width"> 生成验证码图片的宽度 </param>
            /// <param name="Height"> 生成验证码图片的高度 </param>
            /// <param name="FontSize"> 验证码字符大小 </param>
            /// <returns> </returns>
            public byte[] W_CreateValidateGraphic(out string Code, int CodeLength, int Width, int Height, int FontSize)
            {
                string sCode = string.Empty;
                //颜色列表，用于验证码、噪线、噪点
                Color[] oColors ={
             System.Drawing.Color.Black,
             System.Drawing.Color.Red,
             System.Drawing.Color.Blue,
             System.Drawing.Color.Green,
             System.Drawing.Color.Orange,
             System.Drawing.Color.Brown,
             System.Drawing.Color.Brown,
             System.Drawing.Color.DarkBlue
            };
                //字体列表，用于验证码
                string[] oFontNames = { "Times New Roman", "MS Mincho", "Book Antiqua", "Gungsuh", "PMingLiU", "Impact" };
                //验证码的字元集，去掉了一些容易混淆的字符
                //char[] oCharacter = {'2','3','4','5','6','8','9','A','B','C','D','E','F','G','H','J','K', 'L','M','N','P','R','S','T','W','X','Y'};
                char[] oCharacter = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                Random oRnd = new Random();
                Bitmap oBmp = null;
                Graphics oGraphics = null;
                int N1 = 0;
                System.Drawing.Point oPoint1 = default(System.Drawing.Point);
                System.Drawing.Point oPoint2 = default(System.Drawing.Point);
                string sFontName = null;
                Font oFont = null;
                Color oColor = default(Color);
                //生成验证码字串
                for (N1 = 0; N1 <= CodeLength - 1; N1++)
                {
                    sCode += oCharacter[oRnd.Next(oCharacter.Length)];
                }
                oBmp = new Bitmap(Width, Height);
                oGraphics = Graphics.FromImage(oBmp);
                oGraphics.Clear(System.Drawing.Color.White);
                try
                {
                    for (N1 = 0; N1 <= 4; N1++)
                    {
                        //画噪线
                        oPoint1.X = oRnd.Next(Width);
                        oPoint1.Y = oRnd.Next(Height);
                        oPoint2.X = oRnd.Next(Width);
                        oPoint2.Y = oRnd.Next(Height);
                        oColor = oColors[oRnd.Next(oColors.Length)];
                        oGraphics.DrawLine(new Pen(oColor), oPoint1, oPoint2);
                    }
                    float spaceWith = 0, dotX = 0, dotY = 0;
                    if (CodeLength != 0)
                    {
                        spaceWith = (Width - FontSize * CodeLength - 10) / CodeLength;
                    }
                    for (N1 = 0; N1 <= sCode.Length - 1; N1++)
                    {
                        //画验证码字串
                        sFontName = oFontNames[oRnd.Next(oFontNames.Length)];
                        oFont = new Font(sFontName, FontSize, FontStyle.Italic);
                        oColor = oColors[oRnd.Next(oColors.Length)];
                        dotY = (Height - oFont.Height) / 2 + 2;//中心下移2像素
                        dotX = Convert.ToSingle(N1) * FontSize + (N1 + 1) * spaceWith;
                        oGraphics.DrawString(sCode[N1].ToString(), oFont, new SolidBrush(oColor), dotX, dotY);
                    }
                    for (int i = 0; i <= 30; i++)
                    {
                        //画噪点
                        int x = oRnd.Next(oBmp.Width);
                        int y = oRnd.Next(oBmp.Height);
                        Color clr = oColors[oRnd.Next(oColors.Length)];
                        oBmp.SetPixel(x, y, clr);
                    }
                    Code = sCode;
                    //保存图片数据
                    MemoryStream stream = new MemoryStream();
                    oBmp.Save(stream, ImageFormat.Jpeg);
                    //输出图片流
                    return stream.ToArray();
                }
                finally
                {
                    oGraphics.Dispose();
                }
            }

            /// <summary>
            /// 生成6位随机数
            /// </summary>
            /// <param name="CodeLength"> </param>
            /// <returns> </returns>
            public static string W_GetRandomNumber(int CodeLength = 6)
            {
                string Random = "";

                //char[] oCharacter = {'2','3','4','5','6','8','9','A','B','C','D','E','F','G','H','J','K', 'L','M','N','P','R','S','T','W','X','Y'};
                char[] oCharacter = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                //生成验证码字串
                Random oRnd = new Random();
                for (int N1 = 0; N1 <= CodeLength - 1; N1++)
                {
                    Random += oCharacter[oRnd.Next(oCharacter.Length)];
                }
                return Random;
            }
        }
    }
}