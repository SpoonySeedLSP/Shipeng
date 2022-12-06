// See https://aka.ms/new-console-template for more information

using Shipeng.Util;

string wordPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.docx";
string pdfPath = "E:/代码/浙江全品认证有限公司/项目管理操作手册.pdf";
AsposeOfficeHelper.FreeSpireWordToPdf(wordPath, pdfPath);
Console.WriteLine("Hello, World!");
