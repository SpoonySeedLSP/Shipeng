// See https://aka.ms/new-console-template for more information

using Abp.Domain.Entities;
using Shipeng.Util;

//string wordPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.docx";
//string pdfPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.pdf";
//string html = AsposeOfficeHelper.WordToHtml(wordPath);
//AsposeOfficeHelper.FreeSpireWordToPdf(wordPath, pdfPath);

string qrCodeImgPath = "D:\\ynmmnykj\\UplodFiles\\CodeFile\\2023\\code\\二维码02.jpg";
string logPath = "F:\\图片\\刘新玉.png";
//QRCodeHelper.Generate3("https://www.tongkekj.com:8808/authentication.html?id=394007797336900869");
QRCodeHelper.GenerateQrCodeWithLogo("https://www.tongkekj.com:8808/authentication.html?id=394007797336900869", qrCodeImgPath, 500,500, logPath,"1234567890123456");
Console.WriteLine("Hello, World!");
