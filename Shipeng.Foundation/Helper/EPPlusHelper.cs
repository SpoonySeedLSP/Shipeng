using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Shipeng.Util
{
    /// <summary>
    /// 基于EPPlus的excel操作类,仅支持xlsx格式的excel文件
    /// Author:李仕鹏
    /// </summary>
    public class EPPlusHelper
    {
        private static readonly object Obj_Lock = new object();

        #region Excel

        #region EPPlus导出Excel

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="sWebRootFolder">webRoot文件夹</param>
        /// <param name="sFileName">文件名</param>
        /// <param name="sColumnName">自定义列名（不传默认dt列名）</param>
        /// <returns></returns>
        public static byte[] ExportExcel(DataTable dt, string sWebRootFolder, string sFileName, string[] sColumnName, ref string msg)
        {
            try
            {
                if (dt == null || dt.Rows.Count == 0)
                {
                    msg = "没有符合条件的数据！";
                    //  return false;
                }
                //转utf-8
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] buffer = utf8.GetBytes(sFileName);
                sFileName = utf8.GetString(buffer);
                //判断文件夹
                sWebRootFolder = Path.Combine(sWebRootFolder, "ExprotExcel");
                if (!Directory.Exists(sWebRootFolder))
                    Directory.CreateDirectory(sWebRootFolder);
                //删除大于7天的文件
                string[] files = Directory.GetFiles(sWebRootFolder, "*.xlsx", SearchOption.AllDirectories);
                foreach (string item in files)
                {

                    FileInfo f = new FileInfo(item);
                    DateTime now = DateTime.Now;
                    TimeSpan t = now - f.CreationTime;
                    int day = t.Days;
                    if (day > 7)
                    {
                        File.Delete(item);
                    }
                }
                //判断同名文件
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    //判断同名文件创建时间
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    //添加worksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sFileName.Split('.')[0]);
                    //添加表头
                    int column = 1;
                    if (sColumnName.Count() == dt.Columns.Count)
                    {
                        foreach (string cn in sColumnName)
                        {
                            worksheet.Cells[1, column].Value = cn.Trim();
                            worksheet.Cells[1, column].Style.Font.Bold = true;//字体为粗体
                            worksheet.Cells[1, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                            worksheet.Cells[1, column].Style.Fill.PatternType = ExcelFillStyle.Solid;//设置样式类型
                            worksheet.Cells[1, column].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(159, 197, 232));//设置单元格背景色
                            column++;
                        }
                    }
                    else
                    {
                        foreach (DataColumn dc in dt.Columns)
                        {
                            worksheet.Cells[1, column].Value = dc.ColumnName;
                            worksheet.Cells[1, column].Style.Font.Bold = true;//字体为粗体
                            worksheet.Cells[1, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                            worksheet.Cells[1, column].Style.Fill.PatternType = ExcelFillStyle.Solid;//设置样式类型
                            worksheet.Cells[1, column].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(159, 197, 232));//设置单元格背景色
                            column++;
                        }
                    }
                    //添加数据
                    int row = 2;
                    foreach (DataRow dr in dt.Rows)
                    {
                        int col = 1;
                        foreach (DataColumn dc in dt.Columns)
                        {
                            worksheet.Cells[row, col].Value = dr[col - 1].ToString();
                            col++;
                        }
                        row++;
                    }
                    //自动列宽
                    worksheet.Cells.AutoFitColumns();
                    //保存
                    package.Save();
                    return package.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                msg = "生成Excel失败：" + ex.Message;
                return null;
            }
        }

        /// <summary>
        /// datatable导出Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="sWebRootFolder">webRoot文件夹</param>
        /// <param name="sFileName">文件名</param>
        /// <param name="sColumnName">自定义列名（不传默认dt列名）</param>
        /// <param name="msg">失败返回错误信息，有数据返回路径</param>
        /// <returns></returns>
        public static bool DTExportEPPlusExcel(DataTable dt, string sWebRootFolder, string sFileName, string[] sColumnName, ref string msg)
        {
            try
            {
                if (dt == null || dt.Rows.Count == 0)
                {
                    msg = "数据为空";
                    return false;
                }
                //转utf-8
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] buffer = utf8.GetBytes(sFileName);
                sFileName = utf8.GetString(buffer);
                //判断文件夹，不存在创建
                if (!Directory.Exists(sWebRootFolder))
                    Directory.CreateDirectory(sWebRootFolder);
                //删除大于30天的文件，为了保证文件夹不会有过多文件
                string[] files = Directory.GetFiles(sWebRootFolder, "*.xlsx", SearchOption.AllDirectories);
                foreach (string item in files)
                {
                    FileInfo f = new FileInfo(item);
                    DateTime now = DateTime.Now;
                    TimeSpan t = now - f.CreationTime;
                    int day = t.Days;
                    if (day > 30)
                    {
                        File.Delete(item);
                    }
                }
                //判断同名文件
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    //判断同名文件创建时间
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    //添加worksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sFileName.Split('.')[0]);
                    //添加表头
                    int column = 1;
                    if (sColumnName.Count() == dt.Columns.Count)
                    {
                        foreach (string cn in sColumnName)
                        {
                            worksheet.Cells[1, column].Value = cn.Trim();//可以只保留这个，不加样式，导出速度也会加快

                            worksheet.Cells[1, column].Style.Font.Bold = true;//字体为粗体
                            worksheet.Cells[1, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                            worksheet.Cells[1, column].Style.Fill.PatternType = ExcelFillStyle.Solid;//设置样式类型
                            worksheet.Cells[1, column].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(159, 197, 232));//设置单元格背景色
                            column++;
                        }
                    }
                    else
                    {
                        foreach (DataColumn dc in dt.Columns)
                        {
                            worksheet.Cells[1, column].Value = dc.ColumnName;//可以只保留这个，不加样式，导出速度也会加快

                            worksheet.Cells[1, column].Style.Font.Bold = true;//字体为粗体
                            worksheet.Cells[1, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                            worksheet.Cells[1, column].Style.Fill.PatternType = ExcelFillStyle.Solid;//设置样式类型
                            worksheet.Cells[1, column].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(159, 197, 232));//设置单元格背景色
                            column++;
                        }
                    }
                    //添加数据
                    int row = 2;
                    foreach (DataRow dr in dt.Rows)
                    {
                        int col = 1;
                        foreach (DataColumn dc in dt.Columns)
                        {
                            worksheet.Cells[row, col].Value = dr[col - 1].ToString();//这里已知可以减少一层循环，速度会上升
                            col++;
                        }
                        row++;
                    }
                    //自动列宽，由于自动列宽大数据导出严重影响速度，我这里就不开启了，大家可以根据自己情况开启
                    //worksheet.Cells.AutoFitColumns();

                    //保存workbook.
                    package.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                msg = "生成Excel失败：" + ex.Message;
                return false;
            }

        }

        /// <summary>
        /// DataTable导出Excel(单行表头的)
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="columDic">数据表字段字典（不传默认dt列名）,key-->数据表字段,Value-->string[],[0]:字段中文注释,[1]:0.当前列导出后不允许修改 1.当前列导出后允许修改</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, string, ExportFileDTO>> DataTableToExcelAsync(DataTable dt, string savePath, string fileName,
            Dictionary<string, string[]> columDic)
        {
            ExportFileDTO model = new ExportFileDTO();
            if (dt == null || dt.Rows.Count == 0) return Tuple.Create(false, "数据源为空", model);
            try
            {
                await Task.Run(() =>
                {
                    //lock (Obj_Lock)//可以避免多线程并发，如果锁住以后，其实这里跟单线程基本上没啥区别
                    //{
                    Stopwatch watch = new Stopwatch();
                    watch.Start(); //开始监视代码运行时间                      
                                   //转utf-8
                    UTF8Encoding utf8 = new UTF8Encoding();
                    byte[] buffer = utf8.GetBytes(fileName);
                    fileName = utf8.GetString(buffer);
                    //判断文件夹，不存在创建
                    if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
                    //判断同名文件
                    FileInfo file = new FileInfo(Path.Combine(savePath, fileName));
                    if (file.Exists)
                    {
                        //判断同名文件创建时间
                        file.Delete();
                        file = new FileInfo(Path.Combine(savePath, fileName));
                    }
                    using (ExcelPackage package = new ExcelPackage(file))
                    {
                        //添加worksheet
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(fileName.Split('.')[0]);
                        //  worksheet.Cells.Style.ShrinkToFit = true;//单元格自动适应大小
                        #region 添加表头
                        int column = 1;
                        if (columDic == null || columDic.Count <= 0)
                        {
                            #region 按dt列名导出
                            foreach (DataColumn dc in dt.Columns)
                            {
                                worksheet.Cells[1, column].Value = dc.ColumnName;//可以只保留这个，不加样式，导出速度也会加快
                                worksheet.Cells[1, column].Style.Font.Bold = true;//字体为粗体
                                worksheet.Cells[1, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                                worksheet.Cells[1, column].Style.Fill.PatternType = ExcelFillStyle.Solid;//设置样式类型
                                worksheet.Cells[1, column].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(159, 197, 232));//设置单元格背景色
                                column++;
                            }
                            #endregion
                        }
                        else
                        {
                            #region 按设置的字段列导出
                            foreach (KeyValuePair<string, string[]> kv in columDic)
                            {
                                worksheet.Cells[1, column].Style.Font.Size = 14;
                                worksheet.Cells[1, column].Style.Font.Bold = true;//字体为粗体
                                worksheet.Cells[1, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                                worksheet.Cells[1, column].Style.Fill.PatternType = ExcelFillStyle.Solid;//设置样式类型           
                                if (kv.Value[1] == "0")
                                {
                                    worksheet.Cells[1, column].Value = kv.Value[0].Trim() + "(注意此列不可修改)";//可以只保留这个，不加样式，导出速度也会加快
                                    worksheet.Cells[1, column].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 0, 0));//设置单元格背景色
                                }
                                else
                                {
                                    worksheet.Cells[1, column].Value = kv.Value[0].Trim();//可以只保留这个，不加样式，导出速度也会加快
                                    worksheet.Cells[1, column].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(159, 197, 232));//设置单元格背景色
                                }
                                column++;
                            }
                            #endregion
                        }
                        #endregion

                        #region 添加数据
                        int row = 2;
                        foreach (DataRow dr in dt.Rows)
                        {
                            int col = 1;
                            if (columDic == null || columDic.Count <= 0)
                            {
                                foreach (DataColumn dc in dt.Columns)
                                {
                                    worksheet.Cells[row, col].Value = dr[col - 1].ToString();//这里已知可以减少一层循环，速度会上升
                                    col++;
                                }
                            }
                            else
                            {
                                foreach (KeyValuePair<string, string[]> kv in columDic)
                                {
                                    worksheet.Cells[row, col].Value = dr[kv.Key].ToString();//这里已知可以减少一层循环，速度会上升
                                    col++;
                                }
                            }
                            row++;
                        }
                        #endregion

                        package.Save();
                        model.fileContents = FileHelper.StreamToBytes(FileHelper.FileToStream(savePath + fileName));
                        model.contentType = "application/vnd.android.package-archive";
                        model.fileDownloadName = fileName;

                        worksheet.Dispose();
                        package.Dispose();//释放资源，一般也可采用using语句 
                    }
                    //if (File.Exists(savePath + fileName)) File.Delete(savePath + fileName);
                    TimeSpan timespan = watch.Elapsed;//获取当前实例测量得出的总时间
                    watch.Stop(); //停止监视
                    Console.WriteLine("“" + fileName + "”DataTable导出Excel完成，耗时：" + timespan.ToString(@"dd\:hh\:mm\:ss"));
                    //}
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataTable导出Excel发生异常：" + ex.Message);
                return Tuple.Create(false, "DataTable导出Excel发生异常", model);
            }
            return Tuple.Create(true, "", model);
        }


        /// <summary>
        /// Model导出Excel
        /// </summary>
        /// <param name="list">数据源</param>
        /// <param name="sWebRootFolder">webRoot文件夹</param>
        /// <param name="sFileName">文件名</param>
        /// <param name="sColumnName">自定义列名</param>
        /// <param name="msg">失败返回错误信息，有数据返回路径</param>
        /// <returns></returns>
        public static bool ModelExportEPPlusExcel<T>(List<T> myList, string sWebRootFolder, string sFileName, string[] sColumnName, ref string msg)
        {
            try
            {
                if (myList == null || myList.Count == 0)
                {
                    msg = "数据为空";
                    return false;
                }
                //转utf-8
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] buffer = utf8.GetBytes(sFileName);
                sFileName = utf8.GetString(buffer);
                //判断文件夹，不存在创建
                if (!Directory.Exists(sWebRootFolder))
                    Directory.CreateDirectory(sWebRootFolder);
                //删除大于30天的文件，为了保证文件夹不会有过多文件
                string[] files = Directory.GetFiles(sWebRootFolder, "*.xlsx", SearchOption.AllDirectories);
                foreach (string item in files)
                {
                    FileInfo f = new FileInfo(item);
                    DateTime now = DateTime.Now;
                    TimeSpan t = now - f.CreationTime;
                    int day = t.Days;
                    if (day > 30)
                    {
                        File.Delete(item);
                    }
                }
                //判断同名文件
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    //判断同名文件创建时间
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    //添加worksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sFileName.Split('.')[0]);
                    //添加表头
                    int column = 1;
                    if (sColumnName.Count() > 0)
                    {
                        foreach (string cn in sColumnName)
                        {
                            worksheet.Cells[1, column].Value = cn.Trim();//可以只保留这个，不加样式，导出速度也会加快

                            worksheet.Cells[1, column].Style.Font.Bold = true;//字体为粗体
                            worksheet.Cells[1, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                            worksheet.Cells[1, column].Style.Fill.PatternType = ExcelFillStyle.Solid;//设置样式类型
                            worksheet.Cells[1, column].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(159, 197, 232));//设置单元格背景色
                            column++;
                        }
                    }
                    //添加数据
                    int row = 2;
                    foreach (T ob in myList)
                    {
                        int col = 1;
                        foreach (PropertyInfo property in ob.GetType().GetRuntimeProperties())
                        {
                            worksheet.Cells[row, col].Value = property.GetValue(ob);//这里已知可以减少一层循环，速度会上升
                            col++;
                        }
                        row++;
                    }
                    //自动列宽，由于自动列宽大数据导出严重影响速度，我这里就不开启了，大家可以根据自己情况开启
                    //worksheet.Cells.AutoFitColumns();

                    //保存workbook.
                    package.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                msg = "生成Excel失败：" + ex.Message;
                return false;
            }

        }
        #endregion

        #region EPPluse导入

        /// <summary>
        /// 导入Excel批量生成可执行的Update语句
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="tableName">表名</param>
        /// <param name="term">条件字段组</param>
        /// <param name="ignore">忽略字段组</param>
        /// <param name="columDic">数据表字段字典（不传默认dt列名）,key-->数据表字段,Value-->string[],[0]:字段中文注释,[1]:0.当前列导出后不允许修改 1.当前列导出后允许修改</param>
        /// <param name="specificDic">特定要扩充的字段,key.字段名,value.字段值</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, string, List<string>>> ImportExcelCreateUpdateSql(DataTable importDt, string filePath,
            string tableName, string[] term, string[] ignore, Dictionary<string, string[]> columDic, Dictionary<string, object> specificDic)
        {
            List<string> sqlList = new List<string>();
            try
            {
                await Task.Run(() => {
                    lock (Obj_Lock)
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start(); //开始监视代码运行时间 
                        if (importDt == null || importDt.Rows.Count <= 0)
                        {
                            using FileStream excelFile = new FileStream(filePath, FileMode.Open);
                            using ExcelPackage package = new ExcelPackage(excelFile);
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                            importDt = WorksheetToTable(worksheet);
                        }
                        if (importDt != null && importDt.Rows.Count > 0)
                        {
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            foreach (DataColumn col in importDt.Columns)
                            {
                                if (col.ColumnName.Contains("(注意此列不可修改)"))
                                    dic.Add(col.ColumnName.Replace("(注意此列不可修改)", ""), col.ColumnName);
                                else
                                    dic.Add(col.ColumnName, col.ColumnName);
                            }
                            foreach (DataRow row in importDt.Rows)
                            {
                                string updateSql = "update " + tableName + " set ";
                                string fieldSql = "";
                                string whereSql = " where 1=1 ";
                                foreach (KeyValuePair<string, string[]> kv in columDic)
                                {
                                    if (row.Table.Columns.Contains(kv.Value[0]) || row.Table.Columns.Contains(kv.Value[0] + "(注意此列不可修改)"))
                                    {
                                        if ((term == null || term.Length <= 0 || !term.Contains(kv.Key)) && (ignore == null ||
                                             ignore.Length <= 0 || !ignore.Contains(kv.Key)) && kv.Value[1].ToInt() == 1)
                                        {
                                            if (dic[kv.Value[0]] == kv.Value[0])
                                                fieldSql += kv.Key + "='" + row[kv.Value[0]].ToStr() + "',";
                                            else
                                                fieldSql += kv.Key + "='" + row[kv.Value[0] + "(注意此列不可修改)"].ToStr() + "',";
                                        }
                                        else if (term != null && term.Length > 0 && term.Contains(kv.Key))
                                        {
                                            if (dic[kv.Value[0]] == kv.Value[0])
                                                whereSql += " and " + kv.Key + "='" + row[kv.Value[0]].ToStr() + "'";
                                            else
                                                whereSql += " and " + kv.Key + "='" + row[kv.Value[0] + "(注意此列不可修改)"].ToStr() + "'";
                                        }
                                    }
                                }
                                if (specificDic != null && specificDic.Count > 0)
                                {
                                    foreach (KeyValuePair<string, object> kv in specificDic)
                                    {
                                        fieldSql += kv.Key + "='" + kv.Value.ToStr() + "',";
                                    }
                                }
                                updateSql = updateSql + fieldSql.Substring(0, fieldSql.Length - 1) + whereSql;
                                sqlList.Add(updateSql);
                            }
                        }
                        if (File.Exists(filePath)) File.Delete(filePath);
                        TimeSpan timespan = watch.Elapsed;//获取当前实例测量得出的总时间
                        watch.Stop(); //停止监视
                        Console.WriteLine("导入Excel[" + Path.GetFileName(filePath) + "]批量生成可执行的Update语句完成，耗时：" + timespan.ToString(@"dd\:hh\:mm\:ss"));
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (File.Exists(filePath)) File.Delete(filePath);
                Console.WriteLine("导入Excel[" + Path.GetFileName(filePath) + "]批量生成可执行的Update语句发生异常：" + ex.Message);
                return Tuple.Create(false, "导入Excel[" + Path.GetFileName(filePath) + "]批量生成可执行的Update语句发生异常", sqlList);
            }
            return Tuple.Create(true, Path.GetFileName(filePath) + "," + filePath, sqlList);
        }
        #region 转换为datatable
        public static DataTable InputEPPlusByExcelToDT(FileInfo file)
        {
            DataTable dt = new DataTable();
            if (file != null)
            {
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    try
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                        dt = WorksheetToTable(worksheet);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return dt;
        }

        #endregion

        #region 转换为IEnumerable<T>
        /// <summary>
        /// 从Excel中加载数据（泛型）
        /// </summary>
        /// <typeparam name="T">每行数据的类型</typeparam>
        /// <param name="FileName">Excel文件名</param>
        /// <returns>泛型列表</returns>
        public static IEnumerable<T> LoadFromExcel<T>(FileInfo existingFile) where T : new()
        {
            //FileInfo existingFile = new FileInfo(FileName);//如果本地地址可以直接使用本方法，这里是直接拿到了文件
            List<T> resultList = new List<T>();
            Dictionary<string, int> dictHeader = new Dictionary<string, int>();
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int colStart = worksheet.Dimension.Start.Column;  //工作区开始列
                int colEnd = worksheet.Dimension.End.Column;       //工作区结束列
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                //将每列标题添加到字典中
                for (int i = colStart; i <= colEnd; i++)
                {
                    dictHeader[worksheet.Cells[rowStart, i].Value.ToString()] = i;
                }
                List<PropertyInfo> propertyInfoList = new List<PropertyInfo>(typeof(T).GetProperties());
                for (int row = rowStart + 1; row <= rowEnd; row++)
                {
                    T result = new T();
                    //为对象T的各属性赋值
                    foreach (PropertyInfo p in propertyInfoList)
                    {
                        try
                        {
                            ExcelRange cell = worksheet.Cells[row, dictHeader[p.Name]]; //与属性名对应的单元格
                            if (cell.Value == null)
                                continue;
                            switch (p.PropertyType.Name.ToLower())
                            {
                                case "string":
                                    p.SetValue(result, cell.GetValue<string>());
                                    break;
                                case "int16":
                                    p.SetValue(result, cell.GetValue<short>());
                                    break;
                                case "int32":
                                    p.SetValue(result, cell.GetValue<int>());
                                    break;
                                case "int64":
                                    p.SetValue(result, cell.GetValue<long>());
                                    break;
                                case "decimal":
                                    p.SetValue(result, cell.GetValue<decimal>());
                                    break;
                                case "double":
                                    p.SetValue(result, cell.GetValue<double>());
                                    break;
                                case "datetime":
                                    p.SetValue(result, cell.GetValue<DateTime>());
                                    break;
                                case "boolean":
                                    p.SetValue(result, cell.GetValue<bool>());
                                    break;
                                case "byte":
                                    p.SetValue(result, cell.GetValue<byte>());
                                    break;
                                case "char":
                                    p.SetValue(result, cell.GetValue<char>());
                                    break;
                                case "single":
                                    p.SetValue(result, cell.GetValue<float>());
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (KeyNotFoundException)
                        {
                        }
                    }
                    resultList.Add(result);
                }
            }
            return resultList;
        }
        #endregion
        #endregion

        #endregion

        /// <summary>
        /// 打开excel文件
        /// </summary>
        /// <param name="openPath">excel文件路径</param>
        /// <returns>excel对象</returns>
        public ExcelPackage OpenExcel(string openPath)
        {
            // using (ExcelPackage package = new ExcelPackage(new FileStream(path, FileMode.Open)))
            FileStream excelFile = new FileStream(openPath, FileMode.Open);
            ExcelPackage package = new ExcelPackage(excelFile);
            return package;
        }

        /// <summary>
        /// 另存excel文件
        /// </summary>
        /// <param name="package">excel文件对象</param>
        /// <param name="savePath">保存路径</param>
        public void saveExcel(ExcelPackage package, string savePath)
        {
            FileStream excelFile = new FileStream(savePath, FileMode.OpenOrCreate);
            package.SaveAs(excelFile);
            excelFile.Dispose();
            package.Dispose();//释放资源，一般也可采用using语句         
        }

        /// <summary>
        /// 将DataTable内容保存到Xlsx格式文件中
        /// </summary>
        /// <param name="dataTable">数据源</param>
        /// <param name="filePath">输出文件地址</param>
        public static void SaveDataTableToXlsx(DataTable dataTable, string filePath)
        {
            int rowCount = dataTable.Rows.Count;
            int colCount = dataTable.Columns.Count;
            int startRow = 4;
            int startCol = 2;
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                OfficeOpenXml.ExcelWorkbook excelWorkbook = excelPackage.Workbook;
                ExcelWorksheet currentWorksheet = excelWorkbook.Worksheets.Add(dataTable.TableName);
                //生成标题（第一行）、副标题（第二行）
                currentWorksheet.Cells[1, 1, 1, colCount + 1].Merge = true; //合并单元格
                currentWorksheet.Cells[1, 1].Value = dataTable.TableName; //表名
                currentWorksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; //水平居中
                currentWorksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center; //垂直居中
                currentWorksheet.Cells[1, 1].Style.Font.Name = "黑体"; //设置字体
                currentWorksheet.Cells[1, 1].Style.Font.Size = 25; //设置字号
                currentWorksheet.Cells[1, 1].Style.Font.Bold = true; //文字加粗
                currentWorksheet.Row(1).Height = 60; //设置单元格高度
                currentWorksheet.Cells[2, 1, 2, colCount + 1].Merge = true;
                currentWorksheet.Cells[2, 1].Value = string.Format("报表生成于：{0}", DateTime.Now.ToString());
                currentWorksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                currentWorksheet.Cells[2, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center; //垂直居中
                currentWorksheet.Cells[2, 1].Style.Font.Name = "宋体";
                currentWorksheet.Cells[2, 1].Style.Font.Size = 15;
                currentWorksheet.Row(2).Height = 30;
                //生成标题行（第三行）
                currentWorksheet.Cells[3, 1].Value = "序号";
                currentWorksheet.Column(1).Width = 8.38; //设置列宽
                for (int i = 0; i < colCount; i++)
                {
                    currentWorksheet.Cells[3, startCol + i].Value = dataTable.Columns[i].ColumnName;
                    currentWorksheet.Column(startCol + i).Width = 20;
                }
                //调整标题行字体
                using (var range = currentWorksheet.Cells[3, 1, 3, startCol + colCount - 1])
                {
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Font.Name = "宋体";
                    range.Style.Font.Size = 15;
                    range.Style.Font.Bold = true;
                }
                //生成数据（第四行及以后）
                for (int i = 0; i < rowCount; i++)
                {
                    currentWorksheet.Cells[startRow + i, 1].Value = (i + 1).ToString();
                    for (int j = 0; j < colCount; j++)
                    {
                        currentWorksheet.Cells[startRow + i, startCol + j].Value = dataTable.Rows[i][j];
                    }
                }
                //序号列，靠右
                using (var range = currentWorksheet.Cells[startRow, 1, startRow + rowCount - 1, 1])
                {
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                //其他列，居中
                using (var range = currentWorksheet.Cells[startRow, startCol, startRow + rowCount - 1, startCol + colCount - 1])
                {
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                //所有列，其他属性
                using (var range = currentWorksheet.Cells[startRow, 1, startRow + rowCount - 1, startCol + colCount - 1])
                {
                    range.Style.Font.Name = "宋体";
                    range.Style.Font.Size = 11;
                }
                //设置边框，控制台应用程序需要手工添加引用System.Drawing
                using (var range = currentWorksheet.Cells[1, 1, startRow + rowCount - 1, startCol + colCount - 1])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Medium;
                    range.Style.Border.Top.Color.SetColor(Color.Black);
                    range.Style.Border.Right.Style = ExcelBorderStyle.Medium;
                    range.Style.Border.Right.Color.SetColor(Color.Black);
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                    range.Style.Border.Bottom.Color.SetColor(Color.Black);
                    range.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                    range.Style.Border.Left.Color.SetColor(Color.Black);
                }
                //生成Xlsx文档
                excelPackage.SaveAs(new FileInfo(filePath));
            }
        }

        /// <summary>
        /// 将指定的Excel的文件转换成DataTable（Excel的第一个sheet）
        /// </summary>
        /// <param name="fullFielPath">文件的绝对路径</param>
        /// <returns></returns>
        public static DataTable WorksheetToTable(string fullFielPath, int index)
        {
            try
            {
                using FileStream excelFile = new FileStream(fullFielPath, FileMode.Open);
                using ExcelPackage package = new ExcelPackage(excelFile);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[index];
                DataTable dt = WorksheetToTable(worksheet);
                worksheet.Dispose();
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 将worksheet转成datatable
        /// </summary>
        /// <param name="worksheet">待处理的worksheet</param>
        /// <returns>返回处理后的datatable</returns>
        public static DataTable WorksheetToTable(ExcelWorksheet worksheet)
        {
            //获取worksheet的行数
            int rows = worksheet.Dimension.End.Row;
            //获取worksheet的列数
            int cols = worksheet.Dimension.End.Column;
            DataTable dt = new DataTable(worksheet.Name);
            DataRow dr = null;
            for (int i = 1; i <= rows; i++)
            {
                if (i > 1)
                    dr = dt.Rows.Add();

                for (int j = 1; j <= cols; j++)
                {
                    //默认将第一行设置为datatable的标题
                    if (i == 1)
                        dt.Columns.Add(GetString(worksheet.Cells[i, j].Value));
                    //剩下的写入datatable
                    else
                        dr[j - 1] = GetString(worksheet.Cells[i, j].Value);
                }
            }
            return dt;
        }

        private static string GetString(object obj)
        {
            try
            {
                if (obj == null) return "";
                return obj.ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 将一组对象导出成EXCEL
        /// </summary>
        /// <typeparam name="T">要导出对象的类型</typeparam>
        /// <param name="objList">一组对象</param>
        /// <param name="FileName">导出后的文件名</param>
        /// <param name="columnInfo">列名信息</param>
        public static void ExExcel<T>(List<T> objList, string FileName, Dictionary<string, string> columnInfo)
        {
            ExExcel(objList, FileName, columnInfo, null);
        }

        /// <summary>
        /// 将一组对象导出成EXCEL
        /// </summary>
        /// <typeparam name="T">要导出对象的类型</typeparam>
        /// <param name="objList">一组对象</param>
        /// <param name="FileName">导出后的文件名</param>
        /// <param name="columnInfo">列名信息</param>
        /// <param name="other">追加其他内容</param>
        public static void ExExcel<T>(List<T> objList, string FileName, Dictionary<string, string> columnInfo, string other)
        {
            if (columnInfo.Count == 0) { return; }
            if (objList.Count == 0) { return; }
            //生成EXCEL的HTML
            string excelStr = "";
            Type myType = objList[0].GetType();
            //根据反射从传递进来的属性名信息得到要显示的属性
            List<PropertyInfo> myPro = new List<PropertyInfo>();
            foreach (string cName in columnInfo.Keys)
            {
                PropertyInfo p = myType.GetProperty(cName);
                if (p != null)
                {
                    myPro.Add(p);
                    excelStr += columnInfo[cName] + "\t";
                }
            }
            //如果没有找到可用的属性则结束
            if (myPro.Count == 0) { return; }
            excelStr += "\n";
            foreach (T obj in objList)
            {
                foreach (PropertyInfo p in myPro)
                {
                    excelStr += p.GetValue(obj, null) + "\t";
                }
                excelStr += "\n";
            }
            if (!string.IsNullOrEmpty(other))
            {
                excelStr += other;
            }
            //输出EXCEL

        }

        /// <summary> 
        /// 用于excel表格中列号字母转成列索引，从1对应A开始 
        /// </summary> 
        /// <param name="column">列号</param> 
        /// <returns>列索引</returns> 
        private int ColumnToIndex(string column)
        {
            if (!Regex.IsMatch(column.ToUpper(), @"[A-Z]+")) throw new Exception("Invalid parameter");
            int index = 0;
            char[] chars = column.ToUpper().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                index += (chars[i] - 'A' + 1) * (int)Math.Pow(26, chars.Length - i - 1);
            }
            return index;
        }

        /// <summary> 
        /// 用于将excel表格中列索引转成列号字母，从A对应1开始 
        /// </summary> 
        /// <param name="index">列索引</param> 
        /// <returns>列号</returns> 
        private string IndexToColumn(int index)
        {
            if (index <= 0) throw new Exception("Invalid parameter");
            index--;
            string column = string.Empty;
            do
            {
                if (column.Length > 0)
                {
                    index--;
                }
                column = ((char)(index % 26 + 'A')).ToString() + column;
                index = (index - index % 26) / 26;
            } while (index > 0);
            return column;
        }

    }

    /// <summary>
    /// 导出文件模型
    /// </summary>
    public class ExportFileDTO
    {
        public byte[] fileContents { get; set; }

        public string contentType { get; set; }

        public string fileDownloadName { get; set; }
    }

}
