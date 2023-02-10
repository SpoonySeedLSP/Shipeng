using QRCoder;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Drawing.Drawing2D;
using ThoughtWorks.QRCode.Codec;
using ZXing;

namespace Shipeng.Util
{
    /// <summary>
    /// 二维码生成帮助类
    /// </summary>
    public class QRCodeHelper
    {
        /// <summary>
        /// 生成二维码
        /// 引用ZXing生成二维码/条形码
        /// 首先当然是要先注入 ZXing的包了, ZXing.Net 这个包在linux 是不受支持的, 所以这边注入的包:ZXing.Net.Bingdings.imagesSharp.V2
        /// </summary>
        /// <param name="value">内容</param>
        /// <param name="barcodeFormat">生成的类型 CODE_39/ CODE_93/ CODE_128/ QR_CODE   ....</param>
        /// <param name="pathUrl">保存路径</param>
        /// <param name="fileName">文件名字</param>
        /// <param name="width">二维码宽，默认500</param>
        /// <param name="height">二维码高，默认500</param>
        public static void GenerateCode(string value, string barcodeFormat, string pathUrl, string fileName, int width = 500, int height = 500)
        {
            var barcodeFormatType = (BarcodeFormat)System.Enum.Parse(typeof(BarcodeFormat), barcodeFormat);
            var writer = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
            {

                Format = barcodeFormatType,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    DisableECI = true,
                    CharacterSet = "UTF-8",
                    Width = width,
                    Height = height,
                    Margin = 1
                }
            };
            var image = writer.WriteAsImageSharp<Rgba32>(value);
            var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            pathUrl = pathUrl + "/";
            using (var fileStream = File.Create(pathUrl + fileName))
            {
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fileStream);
            }

        }

        #region ThoughtWorks.QRCode.Core
        /// <summary>
        /// 生成二维码：带标题，中间有logo图标的二维码
        /// </summary>
        /// <param name="context">用于生成二维码的数据</param>
        /// <param name="title">二维码标题</param>
        /// <param name="qrCodeImgPath">二维码存储路径</param>
        /// <param name="logoImgPath">log路径</param>
        /// <returns></returns>
        public static string BuildQRCode(string context,string title,string qrCodeImgPath, string logoImgPath)
        {
            using (Image qrCodeImg = GenerateQRCode(context, title))//生成的二维码
            {
                //生成二维码中间logo的图片
                using (Image logoImg = Image.FromFile(logoImgPath))
                {
                    //组合二维码和logo，形成带logo的二维码，并保存
                    CombinImage(qrCodeImg, logoImg).Save(qrCodeImgPath);
                    return qrCodeImgPath;
                }
            }
        }

        /// <summary>
        /// 生成二维码：根据传进去的数据 生成二维码
        /// </summary>
        /// <param name="data">用于生成二维码的数据</param>
        /// <param name="title">二维码标题</param>
        /// <returns></returns>
        public static Image GenerateQRCode(string data,string title)
        {
            //创建编码器，设置编码格式。Byte格式的编码，只要能转成Byte的数据，都可以进行编码，比如中文。NUMERIC 只能编码数字。
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            //大小，值越大生成的二维码图片像素越高
            qrCodeEncoder.QRCodeScale = 5;
            //版本,设置为0主要是防止编码的字符串太长时发生错误
            qrCodeEncoder.QRCodeVersion = 0;
            //生成二维码 Bitmap
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;//错误效验、错误更正(有4个等级)
            qrCodeEncoder.QRCodeBackgroundColor = Color.White;//背景色
            qrCodeEncoder.QRCodeForegroundColor = Color.Black;//前景色
            var pbImg = qrCodeEncoder.Encode(data, System.Text.Encoding.UTF8);
            //增加标题
            pbImg = KiSetText(pbImg, title, 10, 10);
            return pbImg;
        }

        /// <summary>
        /// 给二维码中间添加图片(logo)：将二维码作为背景图片，把小的logo图片放入背景图片的正中央。  
        /// </summary>
        /// <param name="backgroundImg">背景图片(此处为二维码)</param>
        /// <param name="logoImg">logo 图片</param>
        public static Image CombinImage(Image backgroundImg, Image logoImg)
        {
            using (Graphics g = Graphics.FromImage(backgroundImg))
            {
                //画背景(二维码)图片，指定开始坐标为 x:0,y:0，指定背景图片宽高。
                g.DrawImage(backgroundImg, 0, 0, backgroundImg.Width, backgroundImg.Height);
                //logo 图片，重新设置图片宽高
                logoImg = ResizeImage(logoImg, 30, 30, 0);
                //logo四周刷一层红色边框
                //g.FillRectangle(System.Drawing.Brushes.Red, backgroundImg.Width / 2 - img.Width / 2 - 1, backgroundImg.Width / 2 - img.Width / 2 - 1, 32, 32);
                //画logo图片，设置坐标位置，让其居于正中央。
                g.DrawImage(logoImg, backgroundImg.Width / 2 - logoImg.Width / 2, backgroundImg.Width / 2 - logoImg.Width / 2, logoImg.Width, logoImg.Height);
                return backgroundImg;
            }
        }

        /// <summary>
        /// 重新设置图片的宽高
        /// </summary>
        /// <param name="bmp">原始Bitmap</param>
        /// <param name="newW">新的宽度</param>
        /// <param name="newH">新的高度</param>
        /// <param name="Mode">保留着，暂时未用</param>
        /// <returns>重新设置宽高后的图片</returns>
        public static Image ResizeImage(Image bmp, int newW, int newH, int Mode)
        {
            Image b = new Bitmap(newW, newH);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                return b;
            }
        }

        /// <summary>
        /// 增加标题
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="txt">标题</param>
        /// <param name="x">点的水平位置</param>
        /// <param name="y">点的垂直位置</param>
        /// <returns></returns>
        public static Bitmap KiSetText(Bitmap bmp, string txt, int x=0, int y=10)
        {
            Bitmap resizeImage = new Bitmap(bmp.Width, bmp.Height + 40);
            Graphics gfx = Graphics.FromImage(resizeImage);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, 400, 100));
            gfx.DrawImageUnscaled(bmp, 0, 40);
            FontFamily fm = new FontFamily("YaHei");
            Font font = new Font(fm, 24, FontStyle.Regular, GraphicsUnit.Pixel);
            SolidBrush sb = new SolidBrush(Color.Black);
            gfx.DrawString(txt, font, sb, new PointF(x, y));
            gfx.Dispose();
            return resizeImage;
        }
        #endregion



        #region 生成二维码

        /// <summary>
        /// 生成二维码，默认边长为250px
        /// </summary>
        /// <param name="content"> 二维码内容 </param>
        /// <returns> </returns>
        public static string BuildQRCode(string content)
        {
            return BuildQRCode(content, 250, Color.White, Color.Black);
        }

        /// <summary>
        /// 生成二维码,自定义边长
        /// </summary>
        /// <param name="content"> 二维码内容 </param>
        /// <param name="imgSize"> 二维码边长px </param>
        /// <returns> </returns>
        public static string BuildQRCode(string content, int imgSize)
        {
            return BuildQRCode(content, imgSize, Color.White, Color.Black);
        }

        /// <summary>
        /// 生成二维码 注：自定义边长以及颜色
        /// </summary>
        /// <param name="content"> 二维码内容 </param>
        /// <param name="imgSize"> 二维码边长px </param>
        /// <param name="background"> 二维码底色 </param>
        /// <param name="foreground"> 二维码前景色 </param>
        /// <returns> </returns>
        public static string BuildQRCode(string content, int imgSize, Color background, Color foreground)
        {
            return BuildQRCode_Logo(content, imgSize, background, foreground, null);
        }

        /// <summary>
        /// 生成二维码并添加Logo 注：默认生成边长为250px的二维码
        /// </summary>
        /// <param name="content"> 二维码内容 </param>
        /// <param name="logo"> logo图片 </param>
        /// <returns> </returns>
        public static string BuildQRCode_Logo(string content, Bitmap logo)
        {
            return BuildQRCode_Logo(content, 250, Color.White, Color.Black, logo);
        }

        /// <summary>
        /// 生成二维码并添加Logo 注：自定义边长
        /// </summary>
        /// <param name="content"> 二维码内容 </param>
        /// <param name="imgSize"> 二维码边长px </param>
        /// <param name="logo"> logo图片 </param>
        /// <returns> </returns>
        public static string BuildQRCode_Logo(string content, int imgSize, Bitmap logo)
        {
            return BuildQRCode_Logo(content, imgSize, Color.White, Color.Black, logo);
        }

        /// <summary>
        /// 生成二维码并添加Logo 注：自定义边长以及颜色
        /// </summary>
        /// <param name="content"> 二维码内容 </param>
        /// <param name="imgSize"> 二维码边长px </param>
        /// <param name="background"> 二维码底色 </param>
        /// <param name="foreground"> 二维码前景色 </param>
        /// <param name="logo"> logo图片 </param>
        /// <returns> </returns>
        public static string BuildQRCode_Logo(string content, int imgSize, Color background, Color foreground, Bitmap logo)
        {
            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            //QRCode qrCode = new QRCode(qrCodeData);
            //int ppm = imgSize / qrCodeData.ModuleMatrix.Count;
            //Bitmap qrCodeImage = qrCode.GetGraphic(ppm, foreground, background, logo);

            //return qrCodeImage;

            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new(qrCodeData);
            int ppm = imgSize / qrCodeData.ModuleMatrix.Count;
            byte[] byt = qrCode.GetGraphic(ppm);
            return ImgHelper.GetImgbytesString(byt);
            //return ImgHelper.GetbytesImage(byt);
        }

        #endregion 生成二维码

        #region 生成条形码

        /// <summary>
        /// 生成条形码 注：默认宽150px,高50px
        /// </summary>
        /// <param name="content"> 条形码内容 </param>
        /// <returns> </returns>
        public static Image BuildBarCode(string content)
        {
            return BuildBarCode(content, 150, 50);
        }

        /// <summary>
        /// 生成条形码 注：自定义尺寸
        /// </summary>
        /// <param name="content"> 条形码内容 </param>
        /// <param name="width"> 宽度px </param>
        /// <param name="height"> 高度px </param>
        /// <returns> </returns>
        public static Image BuildBarCode(string content, int width, int height)
        {
            throw new Exception("暂不支持!");
        }

        #endregion 生成条形码

        #region 读取码内容

        /// <summary>
        /// 从二维码读取内容
        /// </summary>
        /// <param name="image"> 二维码 </param>
        /// <returns> </returns>
        public static string ReadContent(Bitmap image)
        {
            throw new Exception("暂不支持!");
        }

        #endregion 读取码内容
    }
}