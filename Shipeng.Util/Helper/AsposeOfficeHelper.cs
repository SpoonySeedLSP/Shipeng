using Aspose.Cells;
using System.Data;
using System.Text;
using Word = Microsoft.Office.Interop.Word;

namespace Shipeng.Util
{
    /// <summary>
    /// 使用Aspose组件的Office文件操作帮助类
    /// </summary>
    public class AsposeOfficeHelper
    {
        public const string Key =
            "PExpY2Vuc2U+DQogIDxEYXRhPg0KICAgIDxMaWNlbnNlZFRvPkFzcG9zZSBTY290bGFuZCB" +
            "UZWFtPC9MaWNlbnNlZFRvPg0KICAgIDxFbWFpbFRvPmJpbGx5Lmx1bmRpZUBhc3Bvc2UuY2" +
            "9tPC9FbWFpbFRvPg0KICAgIDxMaWNlbnNlVHlwZT5EZXZlbG9wZXIgT0VNPC9MaWNlbnNlV" +
            "HlwZT4NCiAgICA8TGljZW5zZU5vdGU+TGltaXRlZCB0byAxIGRldmVsb3BlciwgdW5saW1p" +
            "dGVkIHBoeXNpY2FsIGxvY2F0aW9uczwvTGljZW5zZU5vdGU+DQogICAgPE9yZGVySUQ+MTQ" +
            "wNDA4MDUyMzI0PC9PcmRlcklEPg0KICAgIDxVc2VySUQ+OTQyMzY8L1VzZXJJRD4NCiAgIC" +
            "A8T0VNPlRoaXMgaXMgYSByZWRpc3RyaWJ1dGFibGUgbGljZW5zZTwvT0VNPg0KICAgIDxQc" +
            "m9kdWN0cz4NCiAgICAgIDxQcm9kdWN0PkFzcG9zZS5Ub3RhbCBmb3IgLk5FVDwvUHJvZHVj" +
            "dD4NCiAgICA8L1Byb2R1Y3RzPg0KICAgIDxFZGl0aW9uVHlwZT5FbnRlcnByaXNlPC9FZGl" +
            "0aW9uVHlwZT4NCiAgICA8U2VyaWFsTnVtYmVyPjlhNTk1NDdjLTQxZjAtNDI4Yi1iYTcyLT" +
            "djNDM2OGYxNTFkNzwvU2VyaWFsTnVtYmVyPg0KICAgIDxTdWJzY3JpcHRpb25FeHBpcnk+M" +
            "jAxNTEyMzE8L1N1YnNjcmlwdGlvbkV4cGlyeT4NCiAgICA8TGljZW5zZVZlcnNpb24+My4w" +
            "PC9MaWNlbnNlVmVyc2lvbj4NCiAgICA8TGljZW5zZUluc3RydWN0aW9ucz5odHRwOi8vd3d" +
            "3LmFzcG9zZS5jb20vY29ycG9yYXRlL3B1cmNoYXNlL2xpY2Vuc2UtaW5zdHJ1Y3Rpb25zLm" +
            "FzcHg8L0xpY2Vuc2VJbnN0cnVjdGlvbnM+DQogIDwvRGF0YT4NCiAgPFNpZ25hdHVyZT5GT" +
            "zNQSHNibGdEdDhGNTlzTVQxbDFhbXlpOXFrMlY2RThkUWtJUDdMZFRKU3hEaWJORUZ1MXpP" +
            "aW5RYnFGZkt2L3J1dHR2Y3hvUk9rYzF0VWUwRHRPNmNQMVpmNkowVmVtZ1NZOGkvTFpFQ1R" +
            "Hc3pScUpWUVJaME1vVm5CaHVQQUprNWVsaTdmaFZjRjhoV2QzRTRYUTNMemZtSkN1YWoyTk" +
            "V0ZVJpNUhyZmc9PC9TaWduYXR1cmU+DQo8L0xpY2Vuc2U+";
        public static Stream LStream = new MemoryStream(Convert.FromBase64String(Key));

        static AsposeOfficeHelper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// 将DataTable输出为字节数组
        /// </summary>
        /// <param name="dt"> 表格数据 </param>
        /// <returns> Byte数组 </returns>
        public static byte[] DataTableToExcelBytes(DataTable dt)
        {
            Workbook book = new Workbook();
            Worksheet sheet = book.Worksheets[0];
            Cells cells = sheet.Cells;
            int Colnum = dt.Columns.Count;//表格列数
            int Rownum = dt.Rows.Count;//表格行数
            //生成行 列名行
            for (int i = 0; i < Colnum; i++)
            {
                cells[0, i].PutValue(dt.Columns[i].ColumnName);
            }
            //生成数据行
            for (int i = 0; i < Rownum; i++)
            {
                for (int k = 0; k < Colnum; k++)
                {
                    cells[1 + i, k].PutValue(dt.Rows[i][k].ToString());
                }
            }

            //自动行高，列宽
            sheet.AutoFitColumns();
            sheet.AutoFitRows();

            //将DataTable写入内存流
            MemoryStream ms = new MemoryStream();
            book.Save(ms, SaveFormat.Excel97To2003);
            return ms.ToArray();
        }

        /// <summary>
        /// 通过模板导出Excel
        /// </summary>
        /// <param name="templateFile"> 模板 </param>
        /// <param name="dataSource"> 数据源 </param>
        /// <returns> 文件Byte[] </returns>
        public static byte[] ExportExcelByTemplate(string templateFile, params (string SourceName, object Data)[] dataSource)
        {
            if (templateFile.IsNullOrEmpty())
            {
                throw new Exception("模板不能为空");
            }

            if (dataSource.Length == 0)
            {
                throw new Exception("数据源不能为空");
            }

            WorkbookDesigner designer = new WorkbookDesigner
            {
                Workbook = new Workbook(templateFile)
            };
            Workbook workBook = designer.Workbook;

            dataSource.ForEach(aDataSource =>
            {
                designer.SetDataSource(aDataSource.SourceName, aDataSource.Data);
            });
            designer.Process();

            using (MemoryStream stream = new MemoryStream())
            {
                workBook.Save(stream, SaveFormat.Excel97To2003);
                byte[] fileBytes = stream.ToArray();

                return fileBytes;
            }
        }

        /// <summary>
        /// 从excel文件导入数据 注：默认将第一行当作标题行，即不当作数据
        /// </summary>
        /// <param name="fileNmae"> 文件名 </param>
        /// <returns> </returns>
        public static DataTable ReadExcel(string fileNmae)
        {
            Workbook book = new Workbook(fileNmae);
            Worksheet sheet = book.Worksheets[0];
            Cells cells = sheet.Cells;

            return cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, true);
        }

        /// <summary>
        /// 从excel文件导入数据
        /// </summary>
        /// <param name="fileNmae"> 文件名 </param>
        /// <param name="exportColumnName"> 是否将第一行当作标题行 </param>
        /// <returns> </returns>
        public static DataTable ReadExcel(string fileNmae, bool exportColumnName)
        {
            Workbook book = new Workbook(fileNmae);
            Worksheet sheet = book.Worksheets[0];
            Cells cells = sheet.Cells;

            return cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, exportColumnName);
        }

        /// <summary>
        /// 从excel文件字节源导入 注：默认将第一行当作标题行，即不当作数据
        /// </summary>
        /// <param name="fileBytes"> 文件字节源 </param>
        /// <returns> </returns>
        public static DataTable ReadExcel(byte[] fileBytes)
        {
            return ReadExcel(fileBytes, true);
        }

        /// <summary>
        /// 从excel文件字节源导入
        /// </summary>
        /// <param name="fileBytes"> 文件字节源 </param>
        /// <param name="exportColumnName"> 是否将第一行当作标题行 </param>
        /// <returns> </returns>
        public static DataTable ReadExcel(byte[] fileBytes, bool exportColumnName)
        {
            using (MemoryStream ms = new MemoryStream(fileBytes))
            {
                Workbook book = new Workbook(ms);
                Worksheet sheet = book.Worksheets[0];
                Cells cells = sheet.Cells;

                return cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, exportColumnName);
            }
        }

        

        /// <summary>
        /// (Microsoft.Office.Interop.Word)Word转换成PDF(单个文件转换推荐使用)
        /// </summary>
        /// <param name="inputPath">载入完整路径</param>
        /// <param name="outputPath">保存完整路径</param>
        /// <param name="startPage">初始页码（默认为第一页[0]）</param>
        /// <param name="endPage">结束页码（默认为最后一页）</param>
        public static bool OfficeWordToPdf(string inputPath, string outputPath, int startPage = 0, int endPage = 0)
        {
            bool b = true;

            #region 初始化
            //初始化一个application
            Word.Application wordApplication = new Word.Application();
            //初始化一个document
            Word.Document wordDocument = null;
            #endregion

            #region 参数设置（所谓的参数都是根据这个方法来的:ExportAsFixedFormat）
            //word路径
            object wordPath = Path.GetFullPath(inputPath);

            //输出路径
            string pdfPath = Path.GetFullPath(outputPath);

            //导出格式为PDF
            Word.WdExportFormat wdExportFormat = Word.WdExportFormat.wdExportFormatPDF;

            //导出大文件
            Word.WdExportOptimizeFor wdExportOptimizeFor = Word.WdExportOptimizeFor.wdExportOptimizeForPrint;

            //导出整个文档
            Word.WdExportRange wdExportRange = Word.WdExportRange.wdExportAllDocument;

            //开始页码
            int startIndex = startPage;

            //结束页码
            int endIndex = endPage;

            //导出不带标记的文档（这个可以改）
            Word.WdExportItem wdExportItem = Word.WdExportItem.wdExportDocumentContent;

            //包含word属性
            bool includeDocProps = true;

            //导出书签
            Word.WdExportCreateBookmarks paramCreateBookmarks = Word.WdExportCreateBookmarks.wdExportCreateWordBookmarks;

            //默认值
            object paramMissing = Type.Missing;

            #endregion

            #region 转换
            try
            {
                //打开word
                wordDocument = wordApplication.Documents.Open(ref wordPath, ref paramMissing,
                    ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, 
                    ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing,
                    ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, 
                    ref paramMissing, ref paramMissing);
                //转换成指定格式
                if (wordDocument != null)
                {
                    wordDocument.ExportAsFixedFormat(pdfPath, wdExportFormat, false, wdExportOptimizeFor, 
                        wdExportRange, startIndex, endIndex, wdExportItem, includeDocProps, true,
                        paramCreateBookmarks, true, true, false, ref paramMissing);
                }
            }
            catch (Exception ex)
            {
                b = false;
            }
            finally
            {
                //关闭
                if (wordDocument != null)
                {
                    wordDocument.Close(ref paramMissing, ref paramMissing, ref paramMissing);
                    wordDocument = null;
                }

                //退出
                if (wordApplication != null)
                {
                    wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
                    wordApplication = null;
                }
            }
            return b;
            #endregion
        }

        /// <summary>
        /// (Microsoft.Office.Interop.Word)Word将word转pdf
        /// </summary>
        /// <param name="wordPath">word文件路径</param>
        /// <param name="pdfPath">pdf文件存储路径</param>
        public static bool OfficeWordToPdf(object wordPath, string pdfPath)
        {
            bool result = false;
            Word.WdExportFormat wdExportFormatPDF = Word.WdExportFormat.wdExportFormatPDF;
            object missing = Type.Missing;
            Word.ApplicationClass applicationClass = null;
            Word.Document document = null;
            try
            {
                applicationClass = new Word.ApplicationClass();
                document = applicationClass.Documents.Open(ref wordPath, ref missing,
                    ref missing, ref missing, ref missing, ref missing, ref missing, 
                    ref missing, ref missing, ref missing, ref missing, ref missing,
                    ref missing, ref missing, ref missing, ref missing);
                if (document != null)
                {
                    document.ExportAsFixedFormat(pdfPath, wdExportFormatPDF, false,
                        Word.WdExportOptimizeFor.wdExportOptimizeForPrint,
                        Word.WdExportRange.wdExportAllDocument, 0, 0, 
                        Word.WdExportItem.wdExportDocumentContent, true, true, 
                        Word.WdExportCreateBookmarks.wdExportCreateWordBookmarks,
                        true, true, false, ref missing);
                }
                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (document != null)
                {
                    document.Close(ref missing, ref missing, ref missing);
                    document = null;
                }
                if (applicationClass != null)
                {
                    applicationClass.Quit(ref missing, ref missing, ref missing);
                    applicationClass = null;
                }
            }
            return result;
        }

        /// <summary>
        /// (Microsoft.Office.Interop.Word)Word转换成PDF(批量文件转换推荐使用)
        /// </summary>
        /// <param name="inputPath">文件完整路径</param>
        /// <param name="outputPath">保存路径</param>
        public static int OfficeWordToPdfs(string[] inputPaths, string outputPath)
        {
            int count = 0;

            #region 初始化
            //初始化一个application
            Word.Application wordApplication = new Word.Application();
            //初始化一个document
            Word.Document wordDocument = null;
            #endregion

            //默认值
            object paramMissing = Type.Missing;

            for (int i = 0; i < inputPaths.Length; i++)
            {
                #region 参数设置（所谓的参数都是根据这个方法来的:ExportAsFixedFormat）
                //word路径
                object wordPath = Path.GetFullPath(inputPaths[i]);

                //获取文件名
                string outputName = Path.GetFileNameWithoutExtension(inputPaths[i]);

                //输出路径
                string pdfPath = Path.GetFullPath(outputPath + @"\" + outputName + ".pdf");

                //导出格式为PDF
                Word.WdExportFormat wdExportFormat = Word.WdExportFormat.wdExportFormatPDF;

                //导出大文件
                Word.WdExportOptimizeFor wdExportOptimizeFor = Word.WdExportOptimizeFor.wdExportOptimizeForPrint;

                //导出整个文档
                Word.WdExportRange wdExportRange = Word.WdExportRange.wdExportAllDocument;

                //开始页码
                int startIndex = 0;

                //结束页码
                int endIndex = 0;

                //导出不带标记的文档（这个可以改）
                Word.WdExportItem wdExportItem = Word.WdExportItem.wdExportDocumentContent;

                //包含word属性
                bool includeDocProps = true;

                //导出书签
                Word.WdExportCreateBookmarks paramCreateBookmarks = Word.WdExportCreateBookmarks.wdExportCreateWordBookmarks;

                #endregion

                #region 转换
                try
                {
                    //打开word
                    wordDocument = wordApplication.Documents.Open(ref wordPath, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing);
                    //转换成指定格式
                    if (wordDocument != null)
                    {
                        wordDocument.ExportAsFixedFormat(pdfPath, wdExportFormat, false, wdExportOptimizeFor, wdExportRange, startIndex, endIndex, wdExportItem, includeDocProps, true, paramCreateBookmarks, true, true, false, ref paramMissing);
                    }
                    count++;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    //关闭
                    if (wordDocument != null)
                    {
                        wordDocument.Close(ref paramMissing, ref paramMissing, ref paramMissing);
                        wordDocument = null;
                    }
                }
            }

            //退出
            if (wordApplication != null)
            {
                wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
                wordApplication = null;
            }
            return count;
            #endregion
        }

        /// <summary>
        ///  (Microsoft.Office.Interop.Word)Word转换成PDF（带日记）
        /// </summary>
        /// <param name="inputPath">载入完整路径</param>
        /// <param name="outputPath">保存完整路径</param>
        /// <param name="log">转换日记</param>
        /// <param name="startPage">初始页码（默认为第一页[0]）</param>
        /// <param name="endPage">结束页码（默认为最后一页）</param>
        public static void OfficeWordToPpfCreateLog(string inputPath, string outputPath, out string log, int startPage = 0, int endPage = 0)
        {
            log = "success";

            #region 初始化
            //初始化一个application
            Word.Application wordApplication = new Word.Application();
            //初始化一个document
            Word.Document wordDocument = null;
            #endregion

            #region 参数设置~~我去累死宝宝了~~
            //word路径
            object wordPath = Path.GetFullPath(inputPath);

            //输出路径
            string pdfPath = Path.GetFullPath(outputPath);

            //导出格式为PDF
            Word.WdExportFormat wdExportFormat = Word.WdExportFormat.wdExportFormatPDF;

            //导出大文件
            Word.WdExportOptimizeFor wdExportOptimizeFor = Word.WdExportOptimizeFor.wdExportOptimizeForPrint;

            //导出整个文档
            Word.WdExportRange wdExportRange = Word.WdExportRange.wdExportAllDocument;

            //开始页码
            int startIndex = startPage;

            //结束页码
            int endIndex = endPage;

            //导出不带标记的文档（这个可以改）
            Word.WdExportItem wdExportItem = Word.WdExportItem.wdExportDocumentContent;

            //包含word属性
            bool includeDocProps = true;

            //导出书签
            Word.WdExportCreateBookmarks paramCreateBookmarks = Word.WdExportCreateBookmarks.wdExportCreateWordBookmarks;

            //默认值
            object paramMissing = Type.Missing;

            #endregion

            #region 转换
            try
            {
                //打开word
                wordDocument = wordApplication.Documents.Open(ref wordPath, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing, ref paramMissing);
                //转换成指定格式
                if (wordDocument != null)
                {
                    wordDocument.ExportAsFixedFormat(pdfPath, wdExportFormat, false, wdExportOptimizeFor, wdExportRange, startIndex, endIndex, wdExportItem, includeDocProps, true, paramCreateBookmarks, true, true, false, ref paramMissing);
                }
            }
            catch (Exception ex)
            {
                if (ex != null) { log = ex.ToString(); }
            }
            finally
            {
                //关闭
                if (wordDocument != null)
                {
                    wordDocument.Close(ref paramMissing, ref paramMissing, ref paramMissing);
                    wordDocument = null;
                }

                //退出
                if (wordApplication != null)
                {
                    wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
                    wordApplication = null;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            #endregion
        }

        /// <summary>
        /// (Aspose.Words)word转pdf
        /// </summary>
        /// <param name="wordPath">word文件路径</param>
        /// <param name="pdfPath">pdf文件存储路径</param>
        public static void WordToPdf(string wordPath, string pdfPath)
        {
            try
            {
                // 组件授权
                //new Aspose.Words.License().SetLicense(LStream);
                //打开word文件
                Aspose.Words.Document doc = new Aspose.Words.Document(wordPath);
                //验证参数
                if (doc == null) { throw new Exception("Word文件无效"); }
                doc.Save(pdfPath, Aspose.Words.SaveFormat.Pdf);//还可以改成其它格式
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// (FreeSpire)word转pdf
        /// </summary>
        /// <param name="wordPath">word文件路径</param>
        /// <param name="pdfPath">pdf文件存储路径</param>
        public static void FreeSpireWordToPdf(string wordPath, string pdfPath)
        {
            try
            {
                //创建Document类的对象
                var doc = new Spire.Doc.Document();
                //加载word文档
                doc.LoadFromFile(wordPath);
                //将word文档转为Wpdf文档并保存，可选择格式
                doc.SaveToFile(pdfPath, Spire.Doc.FileFormat.PDF);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// (ce.office.extension)Word转Html
        /// </summary>
        /// <param name="wordPath">word文件路径</param>
        public static string WordToHtml(string wordPath)
        {           
            try
            {
                return ce.office.extension.WordHelper.ToHtml(wordPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                return "";
            }
        }

        /// <summary>
        /// (ce.office.extension)Excel转Html
        /// </summary>
        /// <param name="wordPath">word文件路径</param>
        public static string ExcelToHtml(string wordPath)
        {
            try
            {
                return ce.office.extension.ExcelHelper.ToHtml(wordPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                return "";
            }
        }

        //给生成的PDF添加水印
        //public static void SetWatermark(string filePath, string text)
        //{

        //PdfReader pdfReader = null;
        //PdfStamper pdfStamper = null;
        //string tempPath = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "_temp.pdf";
        //try
        //{
        //pdfReader = new PdfReader(filePath);
        //int total = pdfReader.NumberOfPages + 1;

        //using (var fs = new FileStream(tempPath, FileMode.Create))
        //{
        //    pdfStamper = new PdfStamper(pdfReader, fs);
        //    var psize = pdfReader.GetPageSize(1);
        //    float width = psize.Width;
        //    float height = psize.Height;
        //    PdfContentByte content;
        //    BaseFont font = BaseFont.CreateFont(@"C:\WINDOWS\Fonts\SIMFANG.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        //    PdfGState gs = new PdfGState();
        //    int waterMarkNameLenth = text.Length;

        //    for (int i = 1; i < total; i++)
        //    {
        //        var fontLength = 12;
        //        content = pdfStamper.GetOverContent(i);//在内容上方加水印
        //                                               // content = pdfStamper.GetUnderContent(i);//在内容下方加水印
        //                                               // 透明度
        //                                               // gs.FillOpacity = 0.3f;

        //        content.SetGState(gs);
        //        //content.SetGrayFill(0.3f);
        //        //开始写入文本
        //        content.BeginText();
        //        content.SetColorFill(BaseColor.Gray);
        //        content.SetFontAndSize(font, fontLength);
        //        //content.SetTextMatrix(120, 120);
        //        var Margin = 100;//调整水印的间距
        //        var TextSize = new Size(text.Length * fontLength + Margin, text.Length * fontLength + Margin);
        //        var RenderVeiwSize = new Size();
        //        RenderVeiwSize.Width = (int)(width / TextSize.Width) + (width % TextSize.Width != 0 ? 1 : 0);
        //        RenderVeiwSize.Height = (int)(height / TextSize.Height) + (height % TextSize.Height != 0 ? 1 : 0);

        //        for (int h = 0; h < RenderVeiwSize.Height; h++)
        //        {
        //            for (int w = 0; w < RenderVeiwSize.Width; w++)
        //            {
        //                content.ShowTextAligned(Element.ALIGN_CENTER, text, TextSize.Width * w + TextSize.Width / 2, height - TextSize.Height * h - TextSize.Height / 2, 45);
        //            }
        //        }

        //        content.EndText();
        //    }
        //    if (pdfStamper != null)
        //        pdfStamper.Close();

        //    if (pdfReader != null)
        //        pdfReader.Close();

        //    System.IO.File.Copy(tempPath, filePath, true);
        //}
        //System.IO.File.Delete(tempPath);
        //}
        //    catch (Exception ex)
        //    {

        //    }
        //}

    }
}
