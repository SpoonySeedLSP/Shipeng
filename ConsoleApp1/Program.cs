// See https://aka.ms/new-console-template for more information

using Abp.Domain.Entities;
using Shipeng.Util;
using System;

//string wordPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.docx";
//string pdfPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.pdf";
//string html = AsposeOfficeHelper.WordToHtml(wordPath);
//AsposeOfficeHelper.FreeSpireWordToPdf(wordPath, pdfPath);

//string qrCodeImgPath = "D:\\ynmmnykj\\UplodFiles\\CodeFile\\2023\\code\\二维码02.jpg";
//string logPath = "F:\\图片\\刘新玉.png";
////QRCodeHelper.Generate3("https://www.tongkekj.com:8808/authentication.html?id=394007797336900869");
//QRCodeHelper.GenerateQrCode("https://www.tongkekj.com:8808/authentication.html?id=394007797336900869", qrCodeImgPath, 300,300,"1234567890123456");
//Console.WriteLine("Hello, World!");

//MapPoint[] ps = new MapPoint[] { new MapPoint(120.2043, 30.2795), new MapPoint(120.2030, 30.2511), new MapPoint(120.1810, 30.2543), new MapPoint(120.1798, 30.2781), new MapPoint(120.1926, 30.2752) };
//MapPoint n1 = new MapPoint(120.1936, 30.2846);
//MapPoint n2 = new MapPoint(120.1823, 30.2863);
//MapPoint n3 = new MapPoint(120.2189, 30.2712);
//MapPoint y1 = new MapPoint(120.1902, 30.2712);
//MapPoint y2 = new MapPoint(120.1866, 30.2672);
//MapPoint y4 = new MapPoint(120.1869, 30.2718);
//Console.WriteLine(MapHelper.IsPtInPoly(new MapPoint(120.2043, 30.2795), ps));


//规则（花香四季5栋）：102.676643,25.075033
//我的位置（花香四季5栋）：102.70737,25.04347
//位置外（清河局1栋）：102.661309,25.100046
var points = MapHelper.GetDegreeCoordinates(new MapPoint(102.676643, 25.075033),1000);

//Console.WriteLine("判断花香四季是否在误差范围内:"+MapHelper.InLimitDistance(new MapPoint(102.70737, 25.04347), points.ToList(), 1000));
//Console.WriteLine("判断清河居是否在误差范围内:" + MapHelper.InLimitDistance(new MapPoint(102.661309, 25.100046), points.ToList(), 1000));

//Console.WriteLine("连点之间距离公式判断坐标是否在圆内:" + MapHelper.InoutCircle(new MapPoint(102.70737, 25.04347), new MapPoint(102.676643, 25.075033), 1));

//Console.WriteLine("北京天安门坐标是否在圆内:" + MapHelper.InoutCircle(new MapPoint(102.70737, 25.04347), new MapPoint(116.38, 39.90), 1));

double distance = 1000;
Console.WriteLine($"花香四季:{MapHelper.GetDistanceGoogle(new MapPoint(102.676643, 25.075033), new MapPoint(102.70737, 25.04347)) <= distance}");
Console.WriteLine($"清河居:{MapHelper.GetDistanceGoogle(new MapPoint(102.676643, 25.075033), new MapPoint(102.661309, 25.100046)) <= distance}");
Console.WriteLine($"北京天安门:{MapHelper.GetDistanceGoogle(new MapPoint(102.676643, 25.075033), new MapPoint(116.38, 39.90)) <= distance}");

double r = 1;
//Console.WriteLine("花香四季坐标是否在圆内:" + MapHelper.InoutCircle(new MapPoint(102.676643, 25.075033), new MapPoint(102.70737, 25.04347), r));
//Console.WriteLine("清河居坐标是否在圆内:" + MapHelper.InoutCircle(new MapPoint(102.676643, 25.075033), new MapPoint(102.661309, 25.100046), r));
//Console.WriteLine("北京天安门坐标是否在圆内:" + MapHelper.InoutCircle(new MapPoint(102.676643, 25.075033), new MapPoint(116.38, 39.90), r));