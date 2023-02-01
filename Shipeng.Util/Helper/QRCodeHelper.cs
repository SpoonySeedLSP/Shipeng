using QRCoder;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
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