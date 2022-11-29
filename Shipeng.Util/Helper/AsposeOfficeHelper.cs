using Aspose.Cells;
using System.Data;
using System.Text;

namespace Shipeng.Util
{
    /// <summary>
    /// 使用Aspose组件的Office文件操作帮助类
    /// </summary>
    public class AsposeOfficeHelper
    {
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
        /// word转pdf
        /// </summary>
        /// <param name="wordPath">word文件路径</param>
        /// <param name="pdfPath">pdf文件存储路径</param>
        public static void WordToPdf(string wordPath, string pdfPath)
        {
            try
            {
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
