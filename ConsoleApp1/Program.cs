// See https://aka.ms/new-console-template for more information

using Abp.Domain.Entities;
using Shipeng.Util;

//string wordPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.docx";
//string pdfPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.pdf";
//string html = AsposeOfficeHelper.WordToHtml(wordPath);
//AsposeOfficeHelper.FreeSpireWordToPdf(wordPath, pdfPath);

//string qrCodeImgPath = "D:\\ynmmnykj\\UplodFiles\\CodeFile\\2023\\code\\二维码02.jpg";
//string logPath = "F:\\图片\\刘新玉.png";
////QRCodeHelper.Generate3("https://www.tongkekj.com:8808/authentication.html?id=394007797336900869");
//QRCodeHelper.GenerateQrCode("https://www.tongkekj.com:8808/authentication.html?id=394007797336900869", qrCodeImgPath, 300,300,"1234567890123456");
//Console.WriteLine("Hello, World!");

MapPoint[] ps = new MapPoint[] { new MapPoint(120.2043, 30.2795), new MapPoint(120.2030, 30.2511), new MapPoint(120.1810, 30.2543), new MapPoint(120.1798, 30.2781), new MapPoint(120.1926, 30.2752) };
MapPoint n1 = new MapPoint(120.1936, 30.2846);
MapPoint n2 = new MapPoint(120.1823, 30.2863);
MapPoint n3 = new MapPoint(120.2189, 30.2712);
MapPoint y1 = new MapPoint(120.1902, 30.2712);
MapPoint y2 = new MapPoint(120.1866, 30.2672);
MapPoint y4 = new MapPoint(120.1869, 30.2718);
Console.WriteLine(MapHelper.IsPtInPoly(new MapPoint(120.2043, 30.2795), ps));



var points = MapHelper.GetDegreeCoordinates(new MapPoint(102.676643, 25.075033),1000);

Console.WriteLine("2222222:"+MapHelper.InLimitDistance(new MapPoint(102.70737, 25.04347), points.ToList(), 1000));

Console.WriteLine("连点之间距离公式判断坐标是否在圆内:" + MapHelper.InoutCircle(new MapPoint(102.70737, 25.04347), new MapPoint(102.676643, 25.075033), 1));

Console.WriteLine("北京天安门坐标是否在圆内:" + MapHelper.InoutCircle(new MapPoint(102.70737, 25.04347), new MapPoint(116.38, 39.90), 1));
