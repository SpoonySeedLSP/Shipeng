// See https://aka.ms/new-console-template for more information

using Abp.Domain.Entities;
using Shipeng.Util;

//string wordPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.docx";
//string pdfPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.pdf";
//string html = AsposeOfficeHelper.WordToHtml(wordPath);
//AsposeOfficeHelper.FreeSpireWordToPdf(wordPath, pdfPath);

string qrCodeImgPath = "D:\\ynmmnykj\\UplodFiles\\CodeFile\\2023\\code\\二维码.jpg";
string logPath = "F:\\图片\\1.jpg";
QRCodeHelper.BuildQRCode("https://www.tongkekj.com:8031/index.html?id=8888888","1234567890123456", qrCodeImgPath, logPath);
Console.WriteLine("Hello, World!");
