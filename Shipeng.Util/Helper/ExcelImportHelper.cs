using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;

namespace Shipeng.Util
{
    /* 基于NPOI的Excel导入操作类
     * Author:李仕鹏
     * Description：Npoi之导出Word段落，文本，表格，字体等相关样式统一封装
     * Description：2024年7月17日
     */
    public class ExcelImportHelper
    {
        /// <summary>   
        /// 从Excel中获取数据到DataTable   
        /// </summary>   
        /// <param name="filePath">Excel文件全路径(服务器路径)</param>   
        /// <param name="SheetIndex">要获取数据的工作表序号(从0开始)</param>   
        /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始)</param>   
        /// <returns></returns>   
        public static DataTable ToDataTable(string filePath, int SheetIndex = 0, int HeaderRowIndex = 0)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = null;
                if (filePath.IndexOf(".xlsx") == -1)//2003
                {
                    workbook = new HSSFWorkbook(file);
                }
                else
                {
                    workbook = new XSSFWorkbook(file);
                }
                string SheetName = workbook.GetSheetName(SheetIndex);
                return ToDataTable(workbook, SheetName, HeaderRowIndex);
            }
        }

        /// <summary>   
        /// 从Excel中获取数据到DataTable   
        /// </summary>   
        /// <param name="workbook">要处理的工作薄</param>   
        /// <param name="SheetName">要获取数据的工作表名称</param>   
        /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始)</param>   
        /// <returns></returns>   
        public static DataTable ToDataTable(IWorkbook workbook, string SheetName, int HeaderRowIndex)
        {
            ISheet sheet = workbook.GetSheet(SheetName);
            DataTable table = new DataTable();
            try
            {
                IRow headerRow = sheet.GetRow(HeaderRowIndex);
                int cellCount = headerRow.LastCellNum;

                for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                {
                    DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                    table.Columns.Add(column);
                }

                int rowCount = sheet.LastRowNum;
                if (rowCount > 0)
                {
                    #region 循环各行各列,写入数据到DataTable
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        DataRow dataRow = table.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell == null)
                            {
                                dataRow[j] = null;
                            }
                            else
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    dataRow[j] = cell.DateCellValue.ToString("yyyy/MM/dd").Trim();
                                }
                                else
                                {
                                    dataRow[j] = cell.ToString().Trim();
                                }
                            }
                        }
                        table.Rows.Add(dataRow);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                table.Clear();
                table.Columns.Clear();
                table.Columns.Add("出错了");
                DataRow dr = table.NewRow();
                dr[0] = ex.Message;
                table.Rows.Add(dr);
                return table;
            }
            finally
            {
                //sheet.Dispose();   
                workbook = null;
                sheet = null;
            }
            #region 清除最后的空行
            for (int i = table.Rows.Count - 1; i > 0; i--)
            {
                bool isnull = true;
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    if (table.Rows[i][j] != null)
                    {
                        if (table.Rows[i][j].ToString() != "")
                        {
                            isnull = false;
                            break;
                        }
                    }
                }
                if (isnull)
                {
                    table.Rows[i].Delete();
                }
            }
            #endregion

            return table;
        }

        /// <summary>
        /// 从Excel中读入数据到DataTable中
        /// </summary>
        /// <param name="sourceFileNamePath">Excel文件的路径</param>
        /// <param name="sheetName">excel文件中工作表名称</param>
        /// <param name="isHasColumnName">文件是否有列名</param>
        /// <param name="startRowNum">开始行数</param>
        /// <param name="index">excel的第几个sheet</param>
        /// <param name="isPicture">excel中是否有图片</param>
        /// <param name="pictureIndex">excel中图片列索引</param>
        /// <returns>从Excel读取到数据的DataTable结果集</returns>
        public static DataTable ExcelToDataTable(string sourceFileNamePath, string sheetName, bool isHasColumnName = false,
            int startRowNum = 0, int? index = null, bool? isPicture = false, int? pictureIndex = null)
        {
            try
            {
                if (!File.Exists(sourceFileNamePath)) throw new ArgumentException("excel文件的路径不存在或者excel文件没有创建好");
                if (sheetName == null || sheetName.Length == 0) throw new ArgumentException("工作表sheet的名称不能为空");
                bool existPicture = isPicture != null && (bool)isPicture && pictureIndex != null;
                List<IPictureData> pictures = null;

                //根据Excel文件的后缀名创建对应的workbook
                IWorkbook workbook = null;
                //打开文件
                FileStream fs = new FileStream(sourceFileNamePath, FileMode.Open, FileAccess.Read);
                if (sourceFileNamePath.IndexOf(".xlsx") > 0) workbook = new XSSFWorkbook(fs);//2007版本的excel
                else if (sourceFileNamePath.IndexOf(".xls") > 0) workbook = new HSSFWorkbook(fs);//2003版本的excel
                else return null;    //都不匹配或者传入的文件根本就不是excel文件，直接返回

                //获取工作表sheet
                ISheet sheet;
                if (index != null) sheet = workbook.GetSheetAt((int)index);
                else sheet = workbook.GetSheet(sheetName);
                //获取不到，直接返回
                if (sheet == null) return null;

                if (existPicture)
                {
                    //pictures = (List<IPictureData>)workbook.GetAllPictures();
                    pictures = new List<IPictureData>();
                    if (sourceFileNamePath.IndexOf(".xlsx") > 0)
                    {
                        foreach (XSSFPictureData picture in ((XSSFWorkbook)workbook).GetAllPictures())
                        {
                            pictures.Add(picture);
                        }
                    }
                    else
                    {
                        foreach (HSSFPictureData picture in ((HSSFWorkbook)workbook).GetAllPictures())
                        {
                            pictures.Add(picture);
                        }
                    }
                }

                //开始读取的行号
                int StartReadRow = startRowNum;
                DataTable targetTable = new DataTable();

                //表中有列名,则为DataTable添加列名
                if (isHasColumnName)
                {
                    //获取要读取的工作表的第一行
                    IRow columnNameRow = sheet.GetRow(0);//0代表第一行
                    //获取该行的列数(即该行的长度)
                    int CellLength = columnNameRow.LastCellNum;
                    //遍历读取
                    for (int columnNameIndex = 0; columnNameIndex < CellLength; columnNameIndex++)
                    {
                        //不为空，则读入
                        if (columnNameRow.GetCell(columnNameIndex) != null)
                        {
                            //获取该单元格的值
                            string cellValue = columnNameRow.GetCell(columnNameIndex).StringCellValue;
                            if (cellValue != null)
                            {
                                //为DataTable添加列名
                                if (existPicture && pictureIndex == columnNameIndex)
                                {
                                    targetTable.Columns.Add(new DataColumn(cellValue, typeof(byte[])));
                                }
                                else targetTable.Columns.Add(new DataColumn(cellValue));
                            }
                        }
                    }

                    StartReadRow++;
                }
                else
                {
                    IRow columnNameRow = sheet.GetRow(sheet.LastRowNum);
                    int CellLength = columnNameRow.LastCellNum + 20;
                    //遍历读取
                    for (int columnNameIndex = 0; columnNameIndex < CellLength; columnNameIndex++)
                    {
                        //为DataTable添加列名
                        if (existPicture && pictureIndex == columnNameIndex)
                        {
                            targetTable.Columns.Add(new DataColumn(columnNameIndex.ToString(), typeof(byte[])));
                        }
                        else targetTable.Columns.Add(new DataColumn(columnNameIndex.ToString()));
                    }
                }

                #region 开始读取sheet表中的数据
                int dataRowIndex = 0;
                //获取sheet文件中的行数
                int RowLength = isHasColumnName ? sheet.LastRowNum + StartReadRow : sheet.LastRowNum;
                //遍历一行一行地读入
                for (int RowIndex = StartReadRow; RowIndex <= RowLength; RowIndex++)
                {
                    //获取sheet表中对应下标的一行数据
                    IRow currentRow = sheet.GetRow(RowIndex);//RowIndex代表第RowIndex+1行
                    if (currentRow == null) continue; //表示当前行没有数据，则继续
                    //获取第Row行中的列数，即Row行中的长度
                    int currentColumnLength = currentRow.LastCellNum;

                    //创建DataTable的数据行
                    DataRow dataRow = targetTable.NewRow();
                    //遍历读取数据
                    for (int columnIndex = 0; columnIndex < currentColumnLength; columnIndex++)
                    {
                        ICell cell = currentRow.GetCell(columnIndex);
                        if (existPicture && pictureIndex == columnIndex)
                        {
                            // 处理特定列为图片的情况
                            dataRow[columnIndex] = pictures != null && pictures.Count > 0 ?
                                pictures[dataRowIndex]?.Data ?? null : null;
                        }
                        else
                        {
                            //没有数据的单元格默认为空
                            if (currentRow.GetCell(columnIndex) != null)
                            {
                                dataRow[columnIndex] = currentRow.GetCell(columnIndex);
                            }
                        }
                    }
                    //把DataTable的数据行添加到DataTable中
                    targetTable.Rows.Add(dataRow);
                    dataRowIndex++;
                }
                #endregion

                //释放资源
                fs.Close();
                workbook.Close();

                return targetTable;
            }
            catch
            {
                return new DataTable();
            }
        }

    }
}
