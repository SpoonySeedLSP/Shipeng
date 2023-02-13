using QRCoder;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.DrawingCore.Drawing2D;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using ThoughtWorks.QRCode.Codec;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.ZKWeb;

namespace Shipeng.Util
{
    /// <summary>
    /// 二维码生成帮助类
    /// </summary>
    public class QRCodeHelper
    {
        #region ZXing.Net
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

        /// <summary>
        ///  生成条形码
        /// </summary>
        /// <param name="message">条码信息</param>
        /// <param name="gifFileName">生成条码图片文件名</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public static void CreateBarCode(string message, string gifFileName, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            var w = new ZXing.OneD.CodaBarWriter();
            BitMatrix b = w.encode(message, BarcodeFormat.CODE_128, width, height);
            var zzb = new ZXing.ZKWeb.BarcodeWriter();
            zzb.Options = new EncodingOptions()
            {
                Margin = 3,
                PureBarcode = false
            };
            string dir = Path.GetDirectoryName(gifFileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Bitmap b2 = zzb.Write(b);
            b2.Save(gifFileName, ImageFormat.Gif);
            b2.Dispose();
        }

        /// <summary>
        /// 生成二维码返回byte数组
        /// </summary>
        /// <param name="message"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static byte[] CreateCodeBytes(string message, int width = 600, int height = 600)
        {
            int heig = width;
            if (width > height)
            {
                heig = height;
                width = height;
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }
            var w = new QRCodeWriter();
            BitMatrix b = w.encode(message, BarcodeFormat.QR_CODE, width, heig);
            var zzb = new BarcodeWriter();
            zzb.Options = new EncodingOptions()
            {
                Margin = 0,
            };
            Bitmap b2 = zzb.Write(b);
            byte[] bytes = BitmapToArray(b2);
            return bytes;
        }
   
        /// <summary>
        /// 将Bitmap  写为byte[]的方法
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static byte[] BitmapToArray(Bitmap bmp)
        {
            byte[] byteArray = null;
            using (MemoryStream stream = new MemoryStream())
            {
                bmp.Save(stream, ImageFormat.Png);
                byteArray = stream.GetBuffer();
            }
            return byteArray;
        }

        /// <summary>
        /// 生成带Logo的二维码
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="logoPath">Logo 图片</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public static Bitmap GenerateQrCode(string text,string logoPath, int width, int height)
        {
            Bitmap logo = new Bitmap(logoPath);

            //构造二维码写码器
            MultiFormatWriter writer = new MultiFormatWriter();
            Dictionary<EncodeHintType, object> hint = new Dictionary<EncodeHintType, object>();
            hint.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            hint.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);
            //hint.Add(EncodeHintType.MARGIN, 2);//旧版本不起作用，需要手动去除白边

            //生成二维码 
            BitMatrix bm = writer.encode(text, BarcodeFormat.QR_CODE, width + 30, height + 30, hint);
            bm = deleteWhite(bm);
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            Bitmap map = barcodeWriter.Write(bm);

            //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
            int[] rectangle = bm.getEnclosingRectangle();

            //计算插入图片的大小和位置
            int middleW = Math.Min((int)(rectangle[2] / 3), logo.Width);
            int middleH = Math.Min((int)(rectangle[3] / 3), logo.Height);
            int middleL = (map.Width - middleW) / 2;
            int middleT = (map.Height - middleH) / 2;

            Bitmap bmpimg = new Bitmap(map.Width, map.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmpimg))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(map, 0, 0, width, height);

                //白底将二维码插入图片
                g.FillRectangle(Brushes.White, middleL, middleT, middleW, middleH);
                g.DrawImage(logo, middleL, middleT, middleW, middleH);

            }
            return bmpimg;
        }

        /// <summary>
        /// 删除默认对应的空白
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        private static BitMatrix deleteWhite(BitMatrix matrix)
        {

            int[] rec = matrix.getEnclosingRectangle();
            int resWidth = rec[2] + 1;
            int resHeight = rec[3] + 1;

            BitMatrix resMatrix = new BitMatrix(resWidth, resHeight);
            resMatrix.clear();

            for (int i = 0; i < resWidth; i++)
            {

                for (int j = 0; j < resHeight; j++)
                {

                    if (matrix[i + rec[0], j + rec[1]])
                        resMatrix[i, j] = true;
                }

            }
            return resMatrix;
        }

        /// <summary>
        /// 识别二维码/条形码
        /// 读取失败，返回空字符串
        /// </summary>
        /// <param name="filename">指定二维码图片位置</param>
        static string ReadCode(string filename)
        {
            BarcodeReader reader = new BarcodeReader();
            reader.Options.CharacterSet = "UTF-8";
            Bitmap map = new Bitmap(filename);
            Result result = reader.Decode(map);
            return result == null ? "" : result.Text;
        }

        /// <summary>
        /// 生成带下方文字的二维码
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="qrCodeImgPath">二维码保存路径</param>
        /// <param name="w">二维码宽，默认500</param>
        /// <param name="h">二维码高，默认500</param>
        /// <param name="desc">下方的文字</param>
        /// <returns></returns>
        public static void GenerateQrCode(string text, string qrCodeImgPath, int w=500, int h=500, string desc = "")
        {
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            QrCodeEncodingOptions options = new QrCodeEncodingOptions()
            {
                DisableECI = true,//设置内容编码
                CharacterSet = "UTF-8",  //设置二维码的宽度和高度
                Width = w,
                Height = h,
                Margin = 1//设置二维码的边距,单位不是固定像素
            };

            writer.Options = options;
            Bitmap map = writer.Write(text);
            if (!string.IsNullOrWhiteSpace(desc))
            {
                map= AddText(desc, map, w, h);
            }
            //保存成图片
            map.Save(qrCodeImgPath, ImageFormat.Jpeg);
        }

        /// <summary>
        /// 生成带logo，带下方文字的二维码
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="qrCodeImgPath">二维码保存路径</param>
        /// <param name="w">二维码宽，默认500</param>
        /// <param name="h">二维码高，默认500</param>
        /// <param name="logoUrl">log图片路径</param>
        /// <param name="desc">下方的文字</param>
        /// <returns></returns>
        public static void GenerateQrCodeWithLogo(string text, string qrCodeImgPath, int w=500, int h=500, string logoUrl="", string desc = "")
        {
            Bitmap logo = new Bitmap(logoUrl);
            //构造二维码写码器
            MultiFormatWriter writer = new MultiFormatWriter();
            Dictionary<EncodeHintType, object> hint = new Dictionary<EncodeHintType, object>();
            hint.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            hint.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);
            hint.Add(EncodeHintType.MARGIN, 1);

            //生成二维码
            BitMatrix bm = writer.encode(text, BarcodeFormat.QR_CODE, w, h, hint);
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            Bitmap map = barcodeWriter.Write(bm);

            //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
            int[] rectangle = bm.getEnclosingRectangle();

            //计算插入图片的大小和位置
            int middleW = Math.Min((int)(rectangle[2] / 3.5), logo.Width);
            int middleH = Math.Min((int)(rectangle[3] / 3.5), logo.Height);
            int middleL = (map.Width - middleW) / 2;
            int middleT = (map.Height - middleH) / 2;

            // //将img转换成bmp格式，否则后面无法创建Graphics对象
            Bitmap bmpimg = new Bitmap(map.Width, map.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmpimg))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(map, 0, 0, w, h);
                //白底将二维码插入图片
                g.FillRectangle(Brushes.White, middleL, middleT, middleW, middleH);
                g.DrawImage(logo, middleL, middleT, middleW, middleH);
            }

            if (!string.IsNullOrWhiteSpace(desc))
            {
                bmpimg = AddText(desc, bmpimg, w, h);
            }
            //保存成图片
            bmpimg.Save(qrCodeImgPath, ImageFormat.Jpeg);
        }

        /// <summary>
        /// 添加二维码底部描述
        /// </summary>
        /// <param name="desc">描述信息</param>
        /// <param name="qrBitMap"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static Bitmap AddText(string desc, Bitmap qrBitMap, int width, int height)
        {
            var txtHeight = 30;  // 默认一行文字
            Font font = new Font("GB2312", 11, FontStyle.Regular);//设置字体，大小
            SolidBrush sbrush = new SolidBrush(Color.Black); // 设置颜色
            var newMap = new Bitmap(width, height + txtHeight);
            Graphics g = Graphics.FromImage(newMap);
            g.Clear(Color.White);
            var format = StringFormat.GenericDefault;
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;
            g.DrawString(desc, font, sbrush, new RectangleF(0, height, width, txtHeight), format);

            // 合并位图
            g.DrawImage(qrBitMap, new Rectangle(0, 0, width, height));
            g.Dispose();
            return newMap;
        }
        #endregion

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
            using (System.Drawing.Image qrCodeImg = GenerateQRCode(context, title))//生成的二维码
            {
                //生成二维码中间logo的图片
                using (System.Drawing.Image logoImg = System.Drawing.Image.FromFile(logoImgPath))
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
        public static System.Drawing.Image GenerateQRCode(string data,string title)
        {
            //创建编码器，设置编码格式。Byte格式的编码，只要能转成Byte的数据，都可以进行编码，比如中文。NUMERIC 只能编码数字。
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            //大小，值越大生成的二维码图片像素越高
            qrCodeEncoder.QRCodeScale = 10;
            //版本,设置为0主要是防止编码的字符串太长时发生错误
            qrCodeEncoder.QRCodeVersion = 0;
            //生成二维码 Bitmap
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;//错误效验、错误更正(有4个等级)
            qrCodeEncoder.QRCodeBackgroundColor = System.Drawing.Color.White;//背景色
            qrCodeEncoder.QRCodeForegroundColor = System.Drawing.Color.Black;//前景色
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
        public static System.Drawing.Image CombinImage(System.Drawing.Image backgroundImg, System.Drawing.Image logoImg)
        {
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(backgroundImg))
            {
                //画背景(二维码)图片，指定开始坐标为 x:0,y:0，指定背景图片宽高。
                g.DrawImage(backgroundImg, 0, 0, backgroundImg.Width, backgroundImg.Height);
                //logo 图片，重新设置图片宽高
                logoImg = ResizeImage(logoImg, 50, 50, 0);
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
        public static System.Drawing.Image ResizeImage(System.Drawing.Image bmp, int newW, int newH, int Mode)
        {
            System.Drawing.Image b = new System.Drawing.Bitmap(newW, newH);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, new System.Drawing.Rectangle(0, 0, newW, newH), new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.GraphicsUnit.Pixel);
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
        public static System.Drawing.Bitmap KiSetText(System.Drawing.Bitmap bmp, string txt, int x=0, int y=10)
        {
            System.Drawing.Bitmap resizeImage = new System.Drawing.Bitmap(bmp.Width, bmp.Height + 40);
            System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage(resizeImage);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            gfx.FillRectangle(System.Drawing.Brushes.White, new System.Drawing.Rectangle(0, 0, 400, 100));
            gfx.DrawImageUnscaled(bmp, 0, 40);
            System.Drawing.FontFamily fm = new System.Drawing.FontFamily("Microsoft YaHei");
            System.Drawing.Font font = new System.Drawing.Font(fm, 24, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            gfx.DrawString(txt, font, sb, new System.Drawing.PointF(x, y));
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