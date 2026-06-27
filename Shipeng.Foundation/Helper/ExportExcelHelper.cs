using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;
using System.ComponentModel;

namespace Shipeng.Util
{
    /// <summary>
    /// 基于NPOI的导出Excel帮助类
    /// Author:李仕鹏
    /// </summary>
    public static class ExportExcelHelper
    {
        /// <summary>
        ///List object 转 Excel
        /// </summary>
        /// <param name="list"> 数据</param>
        /// <param name="exports"> 导出列</param>
        /// <param name="CompanyId"> 公司id</param>
        /// <param name="UserId"> 会员id</param>
        /// <param name="CompanyName"> 公司名</param>
        /// <param name="UserName"> 会员名</param>
        /// <returns> </returns>
        public static string ToObjectListExcel<T>(List<T> list, List<ExportField> exports, string CompanyId, string UserId, string CompanyName = "Null", string UserName = "Null")
        {
            string url = "";
            if (list.Count > 0)
            {
                HSSFWorkbook workbook = new HSSFWorkbook();

                //, string Password = "911", string Username = "WXJ"
                //if (Username != "WXJ")
                //{
                //    #region 设置密码

                // workbook.WriteProtectWorkbook(Password, Username);

                //    #endregion 设置密码
                //}

                ISheet sheet = workbook.CreateSheet(DateTime.Now.ToString("yyyyMMddHHmmss"));
                IRow row0 = sheet.CreateRow(0);
                int i = 1, j = 0;
                //添加序号
                row0.CreateCell(0).SetCellValue("序号");
                //添加表头
                foreach (ExportField ex in exports)
                {
                    row0.CreateCell(i).SetCellValue(ex.Value);
                    i++;
                }
                i = 1;
                foreach (object data in list)
                {
                    //创建第一列
                    IRow row = sheet.CreateRow(i);
                    j = 1;
                    //添加序号
                    row.CreateCell(0).SetCellValue(i);
                    foreach (ExportField ex in exports)
                    {
                        row.CreateCell(j).SetCellValue(data?.GetPropertyValue(ex.Key)?.ToString() ?? "");
                        j++;
                    }
                    i++;
                }

                string wwwpath = GlobalData.WebRootPathWwwroot;
                string datewwwpath = wwwpath + "\\Upload\\Export\\" + CompanyId + "\\" + DateTime.Now.ToString("yyyyMM");
                if (!Directory.Exists(datewwwpath))
                {
                    Directory.CreateDirectory(datewwwpath);
                }
                string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                string exceptfilepath = datewwwpath + "\\" + filename;
                url = GlobalData.WebRootUrl + "/Upload/Export/" + CompanyId + "/" + DateTime.Now.ToString("yyyyMM") + "/" + filename;
                byte[] buffer = new byte[1024 * 1000];
                using (MemoryStream ms = new MemoryStream())
                {
                    workbook.Write(ms);
                    buffer = ms.GetBuffer();
                    ms.Close();
                }
                using FileStream fileStream = new FileStream(exceptfilepath, FileMode.Create);
                using MemoryStream m = new MemoryStream(buffer);
                m.WriteTo(fileStream);
            }
            else
            {
                throw new BusException("数据不能为空!");
            }
            return url;
        }

        /// <summary>
        ///List object 转 Excel2007,最大支持导出1048576行
        /// </summary>
        /// <param name="list"> 数据</param>
        /// <param name="exports"> 导出列</param>
        /// <param name="CompanyId"> 公司id</param>
        /// <param name="UserId"> 会员id</param>
        /// <param name="CompanyName"> 公司名</param>
        /// <param name="UserName"> 会员名</param>
        /// <returns> </returns>
        public static string ToObjectListExcel2007<T>(List<T> list, List<ExportField> exports, string CompanyId, string UserId, string CompanyName = "Null", string UserName = "Null")
        {
            string url = "";
            if (list.Count > 0)
            {
                // 内存中实时存在10000个对象，超过的实时写入磁盘，保证内存消耗不会过大
                SXSSFWorkbook workbook = new SXSSFWorkbook(10000);
                try
                {
                    ISheet sheet = workbook.CreateSheet(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    IRow row0 = sheet.CreateRow(0);
                    int i = 1, j = 0;
                    //添加序号
                    row0.CreateCell(0).SetCellValue("序号");
                    //添加表头
                    foreach (ExportField ex in exports)
                    {
                        row0.CreateCell(i).SetCellValue(ex.Value);
                        i++;
                    }
                    i = 1;
                    foreach (object data in list)
                    {
                        //创建第一列
                        IRow row = sheet.CreateRow(i);
                        j = 1;
                        //添加序号
                        row.CreateCell(0).SetCellValue(i);
                        foreach (ExportField ex in exports)
                        {
                            row.CreateCell(j).SetCellValue(data?.GetPropertyValue(ex.Key)?.ToString() ?? "");
                            j++;
                        }
                        i++;
                    }

                    string wwwpath = GlobalData.WebRootPathWwwroot;
                    string datewwwpath = wwwpath + "\\Upload\\Export\\" + CompanyId + "\\" + DateTime.Now.ToString("yyyyMM");
                    if (!Directory.Exists(datewwwpath))
                    {
                        Directory.CreateDirectory(datewwwpath);
                    }
                    string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                    string exceptfilepath = datewwwpath + "\\" + filename;
                    url = GlobalData.WebRootUrl + "/Upload/Export/" + CompanyId + "/" + DateTime.Now.ToString("yyyyMM") + "/" + filename;
                    byte[] buffer = new byte[1024 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        workbook.Write(ms);
                        buffer = ms.GetBuffer();
                        ms.Close();
                    }
                    using FileStream fileStream = new FileStream(exceptfilepath, FileMode.Create);
                    using MemoryStream m = new MemoryStream(buffer);
                    m.WriteTo(fileStream);
                    workbook.Dispose();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    workbook.Close();
                }
            }
            else
            {
                throw new BusException("数据不能为空!");
            }
            return url;
        }

        /// <summary>
        /// 获取实体类里面所有的名称、值、DESCRIPTION值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<ExportField> GetProperties<T>(T t)
        {
            string tStr = string.Empty;
            List<ExportField> exportFieldList = new List<ExportField>();
            if (t == null)
            {
                return exportFieldList;
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (properties.Length <= 0)
            {
                return exportFieldList;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                string name = item.Name; //名称
                object value = item.GetValue(t, null);  //值
                string des = ((DescriptionAttribute)Attribute.GetCustomAttribute(item, typeof(DescriptionAttribute)))?.Description ?? null;// 属性描述
                if (des == null)//如果属性描述没有添加则不添加该字段
                {
                    continue;
                }

                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    exportFieldList.Add(
                    new ExportField()
                    {
                        Key = name,
                        Value = des,
                        Required = false
                    }
                    );
                }
                else
                {
                    GetProperties(value);
                }
            }
            return exportFieldList;
        }

        /// <summary>
        /// HSSFWorkbook 导出Excel
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="CompanyId"> 公司id</param>
        /// <param name="UserId"> 会员id</param>
        /// <param name="CompanyName"> 公司名</param>
        /// <param name="UserName"> 会员名</param>
        /// <returns>Excel路径</returns>
        public static string HSSFWorkbook2Excel(HSSFWorkbook workbook, string CompanyId, string UserId, string CompanyName = "Null", string UserName = "Null")
        {

            string wwwpath = GlobalData.WebRootPathWwwroot;  //@"C:";
            string datewwwpath = wwwpath + "\\Upload\\Export\\" + CompanyId + "\\" + DateTime.Now.ToString("yyyyMM");
            if (!Directory.Exists(datewwwpath))
            {
                Directory.CreateDirectory(datewwwpath);
            }
            string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            string exceptfilepath = datewwwpath + "\\" + filename;
            string url = GlobalData.WebRootUrl + "/Upload/Export/" + CompanyId + "/" + DateTime.Now.ToString("yyyyMM") + "/" + filename;
            byte[] buffer = new byte[1024 * 1000];
            using (MemoryStream ms = new())
            {
                workbook.Write(ms);
                buffer = ms.GetBuffer();
                ms.Close();
            }
            using FileStream fileStream = new(exceptfilepath, FileMode.Create);
            using MemoryStream m = new(buffer);
            m.WriteTo(fileStream);

            return url;
        }


        /// <summary>
        /// HSSFWorkbook 导出Excel
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="CompanyId"> 公司id</param>
        /// <param name="UserId"> 会员id</param>
        /// <param name="CompanyName"> 公司名</param>
        /// <param name="UserName"> 会员名</param>
        /// <returns>Excel路径</returns>
        public static string HSSFWorkbook2Excel(IWorkbook workbook, string CompanyId, string UserId, string CompanyName = "Null", string UserName = "Null")
        {
            string wwwpath = GlobalData.WebRootPathWwwroot;
            string datewwwpath = wwwpath + "\\Upload\\Export\\" + CompanyId + "\\" + DateTime.Now.ToString("yyyyMM");
            if (!Directory.Exists(datewwwpath))
            {
                Directory.CreateDirectory(datewwwpath);
            }
            string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            string exceptfilepath = datewwwpath + "\\" + filename;
            string url = GlobalData.WebRootUrl + "/Upload/Export/" + CompanyId + "/" + DateTime.Now.ToString("yyyyMM") + "/" + filename;
            byte[] buffer = new byte[1024 * 1000];
            using (MemoryStream ms = new())
            {
                workbook.Write(ms,true);
                buffer = ms.GetBuffer();
                ms.Close();
            }
            using FileStream fileStream = new(exceptfilepath, FileMode.Create);
            using MemoryStream m = new(buffer);
            m.WriteTo(fileStream);

            return url;
        }
    }
}
