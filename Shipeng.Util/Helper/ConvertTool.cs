using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Shipeng.Util
{
    /// <summary>
    /// 转换工具类
    /// Author:李仕鹏
    /// </summary>
    public static class ConvertTool
    {
        #region object对象转换为实体对象
        /// <summary>
        /// 将object对象转换为实体对象
        /// </summary>
        /// <typeparam name="T">实体对象类名</typeparam>
        /// <param name="asObject">object对象</param>
        /// <returns></returns>
        public static T ConvertObject<T>(object asObject) where T : new()
        {
            //创建实体对象实例
            var t = Activator.CreateInstance<T>();
            if (asObject != null)
            {
                Type type = asObject.GetType();
                //遍历实体对象属性
                foreach (var info in typeof(T).GetProperties())
                {
                    object obj = null;
                    //取得object对象中此属性的值
                    var val = type.GetProperty(info.Name)?.GetValue(asObject);
                    if (val != null)
                    {
                        //非泛型
                        if (!info.PropertyType.IsGenericType)
                            obj = Convert.ChangeType(val, info.PropertyType);
                        else//泛型Nullable<>
                        {
                            Type genericTypeDefinition = info.PropertyType.GetGenericTypeDefinition();
                            if (genericTypeDefinition == typeof(Nullable<>))
                            {
                                obj = Convert.ChangeType(val, Nullable.GetUnderlyingType(info.PropertyType));
                            }
                            else
                            {
                                obj = Convert.ChangeType(val, info.PropertyType);
                            }
                        }
                        info.SetValue(t, obj, null);
                    }
                }
            }
            return t;
        }

        /// <summary>
        /// 将object对象转换为实体对象
        /// </summary>
        /// <typeparam name="T">实体对象类名</typeparam>
        /// <param name="asObject">object对象</param>
        /// <returns></returns>
        public static T ConvertObjectByJson<T>(object asObject) where T : new()
        {
            //.net framework下
            //var serializer = new JavaScriptSerializer();
            //将object对象转换为json字符
            //var json = serializer.Serialize(asObject);          
            //将json字符转换为实体对象
            //var t = serializer.Deserialize<T>(json);
            //return t;

            //.net core 下使用Newtonsoft.Json
            var json = JsonConvert.SerializeObject(asObject);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 将object尝试转为指定对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T ConvertObjToModel<T>(object data) where T : new()
        {
            if (data == null) return new T();
            // 定义集合    
            T result = new T();
            // 获得此模型的类型   
            Type type = typeof(T);
            string tempName = "";
            // 获得此模型的公共属性 
            PropertyInfo[] propertys = result.GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                tempName = pi.Name;  // 检查object是否包含此列    
                // 判断此属性是否有Setter      
                if (!pi.CanWrite) continue;
                try
                {
                    object value = GetPropertyValue(data, tempName);
                    if (value != DBNull.Value)
                    {
                        Type tempType = pi.PropertyType;
                        pi.SetValue(result, GetDataByType(value, tempType), null);

                    }
                }
                catch
                { }
            }
            return result;
        }

        /// <summary>
        /// 获取一个类指定的属性值
        /// </summary>
        /// <param name="info">object对象</param>
        /// <param name="field">属性名称</param>
        /// <returns></returns>
        public static object GetPropertyValue(object info, string field)
        {
            if (info == null) return null;
            Type t = info.GetType();
            IEnumerable<System.Reflection.PropertyInfo> property = from pi in t.GetProperties() where pi.Name.ToLower() == field.ToLower() select pi;
            return property.First().GetValue(info, null);
        }

        /// <summary>
        /// 将数据转为指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data1"></param>
        /// <returns></returns>
        public static object GetDataByType(object data1, Type itype, params object[] myparams)
        {
            object result = new object();
            try
            {
                if (itype == typeof(decimal))
                {
                    result = Convert.ToDecimal(data1);
                    if (myparams.Length > 0)
                    {
                        result = Convert.ToDecimal(Math.Round(Convert.ToDecimal(data1), Convert.ToInt32(myparams[0])));
                    }
                }
                else if (itype == typeof(double))
                {

                    if (myparams.Length > 0)
                    {
                        result = Convert.ToDouble(Math.Round(Convert.ToDouble(data1), Convert.ToInt32(myparams[0])));
                    }
                    else
                    {
                        result = double.Parse(Convert.ToDecimal(data1).ToString("0.00"));
                    }
                }
                else if (itype == typeof(int))
                {
                    result = Convert.ToInt32(data1);
                }
                else if (itype == typeof(DateTime))
                {
                    result = Convert.ToDateTime(data1);
                }
                else if (itype == typeof(Guid))
                {
                    result = new Guid(data1.ToString());
                }
                else if (itype == typeof(string))
                {
                    result = data1.ToString();
                }
            }
            catch
            {
                if (itype == typeof(decimal))
                {
                    result = 0;
                }
                else if (itype == typeof(double))
                {
                    result = 0;
                }
                else if (itype == typeof(int))
                {
                    result = 0;
                }
                else if (itype == typeof(DateTime))
                {
                    result = null;
                }
                else if (itype == typeof(Guid))
                {
                    result = Guid.Empty;
                }
                else if (itype == typeof(string))
                {
                    result = "";
                }
            }
            return result;
        }
        #endregion

        #region 将json转换为DataTable
        /// <summary>
        /// 将json转换为DataTable
        /// </summary>
        /// <param name="strJson">得到的json</param>
        /// <returns></returns>
        public static DataTable JsonToDataTable01(string strJson)
        {
            //转换json格式
            strJson = strJson.Replace(",\"", "*\"").Replace("\":", "\"#").ToString();
            //取出表名   
            var rg = new Regex(@"(?<={)[^:]+(?=:\[)", RegexOptions.IgnoreCase);
            string strName = rg.Match(strJson).Value;
            DataTable tb = null;
            //去除表名   
            strJson = strJson.Substring(strJson.IndexOf("[") + 1);
            strJson = strJson.Substring(0, strJson.IndexOf("]"));
            //获取数据   
            rg = new Regex(@"(?<={)[^}]+(?=})");
            MatchCollection mc = rg.Matches(strJson);
            for (int i = 0; i < mc.Count; i++)
            {
                string strRow = mc[i].Value;
                string[] strRows = strRow.Split('*');
                //创建表   
                if (tb == null)
                {
                    tb = new DataTable();
                    tb.TableName = strName;
                    foreach (string str in strRows)
                    {
                        var dc = new DataColumn();
                        string[] strCell = str.Split('#');
                        if (strCell[0].Substring(0, 1) == "\"")
                        {
                            int a = strCell[0].Length;
                            dc.ColumnName = strCell[0].Substring(1, a - 2);
                        }
                        else
                        {
                            dc.ColumnName = strCell[0];
                        }
                        tb.Columns.Add(dc);
                    }
                    tb.AcceptChanges();
                }
                //增加内容   
                DataRow dr = tb.NewRow();
                for (int r = 0; r < strRows.Length; r++)
                {
                    dr[r] = strRows[r].Split('#')[1].Trim().Replace("，", ",").Replace("：", ":").Replace("\"", "");
                }
                tb.Rows.Add(dr);
                tb.AcceptChanges();
            }
            return tb;
        }

        /// <summary>
        /// json转DataTable
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DataTable JsonToDataTable(string json)
        {
            DataTable table = new DataTable();
            //JsonStr为Json字符串
            JArray array = JsonConvert.DeserializeObject(json) as JArray;//反序列化为数组
            if (array.Count > 0)
            {
                StringBuilder columns = new StringBuilder();
                JObject objColumns = array[0] as JObject;
                //构造表头
                foreach (JToken jkon in objColumns.AsEnumerable<JToken>())
                {
                    string name = ((JProperty)(jkon)).Name;
                    columns.Append(name + ",");
                    table.Columns.Add(name);
                }
                //向表中添加数据
                for (int i = 0; i < array.Count; i++)
                {
                    DataRow row = table.NewRow();
                    JObject obj = array[i] as JObject;
                    foreach (JToken jkon in obj.AsEnumerable<JToken>())
                    {

                        string name = ((JProperty)(jkon)).Name;
                        string value = ((JProperty)(jkon)).Value.ToString();
                        row[name] = value;
                    }
                    table.Rows.Add(row);
                }
            }
            return table;
        }

        /// <summary>
        /// IList<T>转DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
            TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        /// <summary>
        /// 将json字符串反序列化为字典类型
        /// </summary>
        /// <typeparam name="TKey">字典key</typeparam>
        /// <typeparam name="TValue">字典value</typeparam>
        /// <param name="jsonStr">json字符串</param>
        /// <returns>字典数据</returns>
        public static Dictionary<TKey, TValue> DeserializeStringToDictionary<TKey, TValue>(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return new Dictionary<TKey, TValue>();

            Dictionary<TKey, TValue> jsonDict = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(jsonStr);

            return jsonDict;

        }
        #endregion

        /// <summary>
        /// datatow 转换Datatable
        /// </summary>
        /// <param name="drArr"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(DataRow[] rows)
        {
            if (rows == null || rows.Length == 0) return null;
            DataTable tmp = rows[0].Table.Clone(); //复制DataRow的表结构
            foreach (DataRow row in rows)
            {
                tmp.ImportRow(row); // 将DataRow添加到DataTable中
            }
            return tmp;
        }

        /// <summary>
        /// 根据json字符串生成可执行的update-sql语句
        /// </summary>
        /// <param name="json">json数据</param>
        /// <param name="tableName">表名</param>
        /// <param name="term">条件字段组</param>
        /// <param name="ignore">忽略字段组</param>
        /// <returns></returns>
        public static string[] CreateUpdateSqlByJson(string json, string tableName, string[] term, string[] ignore)
        {
            try
            {
                DataTable data = JsonConvert.DeserializeObject<DataTable>(json);
                if (data == null || data.Rows.Count <= 0) return null;
                string[] sqls = new string[data.Rows.Count];
                List<string> colums = new List<string>();
                foreach (DataColumn col in data.Columns)
                {
                    colums.Add(col.ColumnName);//获取到DataColumn列对象的列名                  
                }
                int i = 0;
                string sql = "";
                string whereSql = "";
                foreach (DataRow row in data.Rows)
                {
                    sql = "update " + tableName + " set ";
                    whereSql = " where 1=1";
                    foreach (string colum in colums)
                    {
                        if (colum == null || colum.Length <= 0 || !ignore.Contains(colum)) sql += colum + "='" + row[colum].ToStr() + "',";
                        if (term != null && term.Length > 0 && term.Contains(colum)) whereSql += " and " + colum + "='" + row[colum].ToStr() + "'";
                    }
                    if (sql != "update " + tableName + " set ") sqls[i] = sql.Substring(0, sql.Length - 1) + whereSql;
                    i++;
                }
                return sqls;
            }
            catch (Exception ex)
            {
                throw new Exception("根据json字符串生成可执行的sql语句发生异常：" + ex.Message);
            }
        }

        public static DataRow[] GetTableRows(DataTable dtAllEas, int PageIndex, int PageSize)
        {
            var rows = dtAllEas.Rows.Cast<DataRow>();
            var curRows = rows.Skip(PageIndex).Take(PageSize).ToArray();
            return curRows;
        }

        /// <summary>
        /// DataTable分页并取出指定页码的数据
        /// </summary>
        /// <param name="dtAll">DataTable</param>
        /// <param name="pageNo">页码,注意：从1开始</param>
        /// <param name="pageSize">每页条数</param>
        /// <returns>指定页码的DataTable数据</returns>
        public static DataTable GetOnePageTable(DataTable dtAll, int pageNo, int pageSize)
        {
            var totalCount = dtAll.Rows.Count;
            var totalPage = GetTotalPage(totalCount, pageSize);
            var currentPage = pageNo;
            currentPage = (currentPage > totalPage ? totalPage : currentPage);//如果PageNo过大，则较正PageNo=PageCount
            currentPage = (currentPage <= 0 ? 1 : currentPage);//如果PageNo<=0，则改为首页
            //----克隆表结构到新表
            var onePageTable = dtAll.Clone();
            //----取出1页数据到新表
            var rowBegin = (currentPage - 1) * pageSize;
            var rowEnd = currentPage * pageSize;
            rowEnd = (rowEnd > totalCount ? totalCount : rowEnd);
            for (var i = rowBegin; i <= rowEnd - 1; i++)
            {
                var newRow = onePageTable.NewRow();
                var oldRow = dtAll.Rows[i];
                foreach (DataColumn column in dtAll.Columns)
                {
                    newRow[column.ColumnName] = oldRow[column.ColumnName];
                }
                onePageTable.Rows.Add(newRow);
            }
            return onePageTable;
        }

        /// <summary>
        /// 返回分页后的总页数
        /// </summary>
        /// <param name="totalCount">总记录条数</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <returns>总页数</returns>
        public static int GetTotalPage(int totalCount, int pageSize)
        {
            var totalPage = (totalCount / pageSize) + (totalCount % pageSize > 0 ? 1 : 0);
            return totalPage;
        }

        /// <summary>
        /// DataTable转成List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> ToDataList<T>(this DataTable dt)
        {
            var list = new List<T>();
            var plist = new List<PropertyInfo>(typeof(T).GetProperties());
            if (dt == null || dt.Rows.Count == 0) return null;
            foreach (DataRow item in dt.Rows)
            {
                T s = Activator.CreateInstance<T>();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    PropertyInfo info = plist.Find(p => p.Name == dt.Columns[i].ColumnName);
                    if (info != null)
                    {
                        try
                        {
                            if (!Convert.IsDBNull(item[i]))
                            {
                                object v = null;
                                if (info.PropertyType.ToString().Contains("System.Nullable"))
                                {
                                    v = Convert.ChangeType(item[i], Nullable.GetUnderlyingType(info.PropertyType));
                                }
                                else
                                {
                                    v = Convert.ChangeType(item[i], info.PropertyType);
                                }
                                info.SetValue(s, v, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("字段[" + info.Name + "]转换出错," + ex.Message);
                        }
                    }
                }
                list.Add(s);
            }
            return list;
        }

        /// <summary> 
        /// 利用反射将DataTable转换为List<T>对象
        /// </summary> 
        /// <param name="dt">DataTable 对象</param> 
        /// <returns>List<T>集合</returns> 
        public static List<T> DataTableToList<T>(DataTable dt) where T : class, new()
        {
            // 定义集合 
            List<T> ts = new List<T>();
            //定义一个临时变量 
            string tempName = string.Empty;
            //遍历DataTable中所有的数据行 
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性 
                PropertyInfo[] propertys = t.GetType().GetProperties();
                //遍历该对象的所有属性 
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;//将属性名称赋值给临时变量 
                    //检查DataTable是否包含此列（列名==对象的属性名）  
                    if (dt.Columns.Contains(tempName))
                    {
                        //取值 
                        object value = dr[tempName];
                        //如果非空，则赋给对象的属性 
                        if (value != DBNull.Value)
                        {
                            pi.SetValue(t, value, null);
                        }
                    }
                }
                //对象添加到泛型集合中 
                ts.Add(t);
            }
            return ts;
        }

        /// <summary>
        /// datatable转换为List<T>集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> DataTableToListT<T>(DataTable table)
        {
            var list = new List<T>();
            foreach (DataRow item in table.Rows)
            {
                list.Add(DataRowToModel<T>(item));
            }
            return list;
        }

        public static T DataRowToModel<T>(DataRow row)
        {
            T model;
            var type = typeof(T);
            var modelType = GetModelType(type);
            switch (modelType)
            {
                //值类型
                case ModelType.Struct:
                    {
                        model = default(T);
                        if (row[0] != null)
                            model = (T)row[0];
                    }
                    break;
                //值类型
                case ModelType.Enum:
                    {
                        model = default(T);
                        if (row[0] != null)
                        {
                            var fiType = row[0].GetType();
                            if (fiType == typeof(int))
                            {
                                model = (T)row[0];
                            }
                            else if (fiType == typeof(string))
                            {
                                model = (T)System.Enum.Parse(typeof(T), row[0].ToString());
                            }
                        }
                    }
                    break;
                //引用类型 c#对string也当做值类型处理
                case ModelType.String:
                    {
                        model = default(T);
                        if (row[0] != null)
                            model = (T)row[0];
                    }
                    break;
                //引用类型 直接返回第一行第一列的值
                case ModelType.Object:
                    {
                        model = default(T);
                        if (row[0] != null)
                            model = (T)row[0];
                    }
                    break;
                //引用类型
                case ModelType.Else:
                    {
                        //引用类型 必须对泛型实例化
                        model = Activator.CreateInstance<T>();
                        //获取model中的属性
                        var modelPropertyInfos = type.GetProperties();
                        //遍历model每一个属性并赋值DataRow对应的列
                        foreach (var pi in modelPropertyInfos)
                        {
                            //获取属性名称
                            var name = pi.Name;
                            if (!row.Table.Columns.Contains(name) || row[name] == null) continue;
                            var piType = GetModelType(pi.PropertyType);
                            switch (piType)
                            {
                                case ModelType.Struct:
                                    {
                                        var value = Convert.ChangeType(row[name], pi.PropertyType);
                                        pi.SetValue(model, value, null);
                                    }
                                    break;
                                case ModelType.Enum:
                                    {
                                        var fiType = row[0].GetType();
                                        if (fiType == typeof(int))
                                        {
                                            pi.SetValue(model, row[name], null);
                                        }
                                        else if (fiType == typeof(string))
                                        {
                                            var value = (T)System.Enum.Parse(typeof(T), row[name].ToString());
                                            if (value != null)
                                                pi.SetValue(model, value, null);
                                        }
                                    }
                                    break;
                                case ModelType.String:
                                    {
                                        var value = Convert.ChangeType(row[name], pi.PropertyType);
                                        pi.SetValue(model, value, null);
                                    }
                                    break;
                                case ModelType.Object:
                                    {
                                        pi.SetValue(model, row[name], null);
                                    }
                                    break;
                                case ModelType.Else:
                                    throw new Exception("不支持该类型转换");
                                default:
                                    throw new Exception("未知类型");
                            }
                        }
                    }
                    break;
                default:
                    model = default(T);
                    break;
            }
            return model;
        }

        /// <summary>
        /// 类型枚举
        /// </summary>
        private enum ModelType
        {
            //值类型
            Struct,
            Enum,
            //引用类型
            String,
            Object,
            Else
        }
        private static ModelType GetModelType(Type modelType)
        {
            //值类型
            if (modelType.IsEnum)
            {
                return ModelType.Enum;
            }
            //值类型
            if (modelType.IsValueType)
            {
                return ModelType.Struct;
            }
            //引用类型 特殊类型处理
            if (modelType == typeof(string))
            {
                return ModelType.String;
            }
            //引用类型 特殊类型处理
            return modelType == typeof(object) ? ModelType.Object : ModelType.Else;
        }

        /// <summary>
        /// DataTable转成Dto
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static T ToDataDto<T>(this DataTable dt)
        {
            T s = Activator.CreateInstance<T>();
            if (dt == null || dt.Rows.Count == 0)
            {
                return s;
            }
            var plist = new List<PropertyInfo>(typeof(T).GetProperties());
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                PropertyInfo info = plist.Find(p => p.Name == dt.Columns[i].ColumnName);
                if (info != null)
                {
                    try
                    {
                        if (!Convert.IsDBNull(dt.Rows[0][i]))
                        {
                            object v = null;
                            if (info.PropertyType.ToString().Contains("System.Nullable"))
                            {
                                v = Convert.ChangeType(dt.Rows[0][i], Nullable.GetUnderlyingType(info.PropertyType));
                            }
                            else
                            {
                                v = Convert.ChangeType(dt.Rows[0][i], info.PropertyType);
                            }
                            info.SetValue(s, v, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("字段[" + info.Name + "]转换出错," + ex.Message);
                    }
                }
            }
            return s;
        }

        /// <summary>
        /// 将泛类型集合List类转换成DataTable
        /// </summary>
        /// <param name="list">泛类型集合</param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(List<T> entitys)
        {
            //检查实体集合不能为空
            if (entitys == null || entitys.Count < 1)
            {
                return null;
                //throw new Exception("需转换的集合为空");
            }
            //取出第一个实体的所有Propertie
            //Type entityType = entitys[0].GetType();
            PropertyInfo[] entityProperties = entitys[0].GetType().GetProperties();

            //生成DataTable的structure
            //生产代码中，应将生成的DataTable结构Cache起来，此处略
            DataTable dt = new DataTable("temp");
            for (int i = 0; i < entityProperties.Length; i++)
            {
                //dt.Columns.Add(entityProperties[i].Name, entityProperties[i].PropertyType);
                dt.Columns.Add(entityProperties[i].Name);
            }
            //将所有entity添加到DataTable中
            foreach (object entity in entitys)
            {
                //检查所有的的实体都为同一类型
                //if (entity.GetType() != entityType)
                //{
                //    throw new Exception("要转换的集合元素类型不一致");
                //}
                object[] entityValues = new object[entityProperties.Length];
                for (int i = 0; i < entityProperties.Length; i++)
                {
                    entityValues[i] = entityProperties[i].GetValue(entity, null);
                }
                dt.Rows.Add(entityValues);
            }
            return dt;
        }

        /// <summary>
        /// 将实体集合转换为DataTable
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">实体集合</param>
        public static DataTable ToDataTable<T>(List<T> entities)
        {
            var result = CreateTable<T>();
            FillData(result, entities);
            return result;
        }

        /// <summary>
        /// 创建表
        /// </summary>
        private static DataTable CreateTable<T>()
        {
            var result = new DataTable();
            var type = typeof(T);
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyType = property.PropertyType;
                if ((propertyType.IsGenericType) && (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    propertyType = propertyType.GetGenericArguments()[0];
                result.Columns.Add(property.Name, propertyType);
            }
            return result;
        }

        /// <summary>
        /// 填充数据
        /// </summary>
        private static void FillData<T>(DataTable dt, IEnumerable<T> entities)
        {
            if (entities == null || entities.Count() <= 0) return;
            foreach (var entity in entities)
            {
                dt.Rows.Add(CreateRow(dt, entity));
            }
        }

        /// <summary>
        /// 创建行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static DataRow CreateRow<T>(DataTable dt, T entity)
        {
            DataRow row = dt.NewRow();
            var type = typeof(T);
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                row[property.Name] = property.GetValue(entity) ?? DBNull.Value;
            }
            return row;
        }

        #region HashTable转换为DataTable互转DataTable转换为HashTable

        /// <summary>
        /// HashTable转换为DataTable
        /// </summary>
        /// <param name="ht"></param>
        /// <returns></returns>
        public static DataTable HashTableToDataTable(Hashtable ht)
        {
            try
            {
                //创建DataTable
                DataTable dt = new DataTable();
                //创建新列
                DataColumn dc1 = dt.Columns.Add("dc1", typeof(string));
                DataColumn dc2 = dt.Columns.Add("dc2", typeof(string));
                //将HashTable中的值添加到DataTable中
                foreach (DictionaryEntry element in ht)
                {
                    DataRow dr = dt.NewRow();
                    dr["dc1"] = (string)element.Key;
                    dr["dc2"] = element.Value.ToString();
                    dt.Rows.Add(dr);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DataTablez转Hashtable
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Hashtable GetHashTable(DataTable dt)
        {
            try
            {
                if (dt != null)
                {
                    Hashtable hash = new Hashtable();
                    foreach (DataRow row in dt.Rows)
                    {
                        hash.Add(row["dc1"], row["dc2"]);
                    }
                    return hash;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///Hashtable转object实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Hashtable2Object<T>(Hashtable source)
        {
            T obj = Activator.CreateInstance<T>();
            object tv;
            PropertyInfo[] ps = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo p in ps)
            {
                if (source.ContainsKey(p.Name))
                {
                    tv = source[p.Name];
                    if (tv == null) p.SetValue(obj, tv, null);
                    else
                    {
                        if (p.PropertyType.IsArray)//数组类型,单独处理
                        {
                            p.SetValue(obj, tv, null);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tv.ToString()))//空值
                                tv = p.PropertyType.IsValueType ? Activator.CreateInstance(p.PropertyType) : null;//值类型
                            else
                                tv = TypeDescriptor.GetConverter(p.PropertyType).ConvertFromString(tv.ToString());//创建对象
                            p.SetValue(obj, tv, null);
                        }
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// Int32转日期时间
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(int d)
        {
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            startTime = startTime.AddSeconds(d).ToLocalTime();
            return startTime;
        }

        /// <summary>
        /// 日期时间转Int32
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int ConvertDateTimeToInt32(string dt)
        {
            DateTime dt1 = new DateTime(1970, 1, 1, 8, 0, 0);
            DateTime dt2 = Convert.ToDateTime(dt);
            return Convert.ToInt32((dt2 - dt1).TotalSeconds);
        }



        #endregion

        /// <summary>
        /// 根据列名获取列值
        /// </summary>
        /// <param name="colName">列名</param>
        /// <param name="item">对象</param>
        /// <returns></returns>
        public static string ObjValue2<T>(string colName, T item) where T : new()
        {
            object obj = "";
            Type type = typeof(T);
            var proInfo = type.GetProperty(colName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (proInfo != null)
            {
                obj = proInfo.GetValue(item, null);
            }
            return obj.ToString();
        }

        /// <summary>
        /// Hashtable转object实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T HashtableToObject<T>(Hashtable source)
        {
            T obj = Activator.CreateInstance<T>();
            object tv;
            PropertyInfo[] ps = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo p in ps)
            {
                if (source.ContainsKey(p.Name))
                {
                    tv = source[p.Name];
                    if (p.PropertyType.IsArray)//数组类型,单独处理
                    {
                        p.SetValue(obj, tv, null);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(tv.ToString()))//空值
                            tv = p.PropertyType.IsValueType ? Activator.CreateInstance(p.PropertyType) : null;//值类型
                        else
                            tv = TypeDescriptor.GetConverter(p.PropertyType).ConvertFromString(tv.ToString());//创建对象
                        p.SetValue(obj, tv, null);
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// 实体对象Object转HashTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Hashtable ObjectToHashtable(object obj)
        {
            Hashtable hash = new Hashtable();
            PropertyInfo[] ps = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo p in ps)
            {
                hash.Add(p.Name, p.GetValue(obj));
            }
            return hash;
        }

        /// <summary>
        /// Hashtable转JSON
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HashtableToJson(Hashtable data)
        {
            try
            {
                if (data == null || data.Count <= 0) return "";
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                foreach (object key in data.Keys)
                {
                    object value = data[key];
                    sb.Append("\"");
                    sb.Append(key);
                    sb.Append("\":\"");
                    if (!string.IsNullOrEmpty(value.ToString()) && value != DBNull.Value)
                    {
                        sb.Append(value).Replace("\\", "/");
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                    sb.Append("\",");
                }
                sb = sb.Remove(sb.Length - 1, 1);
                sb.Append("}");
                return sb.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        #region 通过反射实现两个相同结构实体类的转换
        public static T2 CopyToModel<T1, T2>(T1 source)
        {
            T2 model = default(T2);
            PropertyInfo[] pi = typeof(T2).GetProperties();
            PropertyInfo[] pi1 = typeof(T1).GetProperties();
            model = Activator.CreateInstance<T2>();
            for (int i = 0; i < pi.Length; i++)
            {
                pi[i].SetValue(model, pi1[i].GetValue(source, null), null);
            }
            return model;
        }

        public static List<T2> CopyToList<T1, T2>(List<T1> source)
        {
            List<T2> t2List = new List<T2>();
            T2 model = default(T2);
            PropertyInfo[] pi = typeof(T2).GetProperties();
            PropertyInfo[] pi1 = typeof(T1).GetProperties();
            foreach (T1 t1Model in source)
            {
                model = Activator.CreateInstance<T2>();
                for (int i = 0; i < pi.Length; i++)
                {
                    pi[i].SetValue(model, pi1[i].GetValue(t1Model, null), null);
                }
                t2List.Add(model);
            }
            return t2List;
        }

        /// <summary>
        /// 两个实体之间相同属性的映射
        /// </summary>
        /// <typeparam name="R">目标实体</typeparam>
        /// <typeparam name="T">数据源实体</typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static R Mapping<R, T>(T model)
        {
            R result = Activator.CreateInstance<R>();
            foreach (PropertyInfo info in typeof(R).GetProperties())
            {
                PropertyInfo pro = typeof(T).GetProperty(info.Name);
                if (pro != null)
                    info.SetValue(result, pro.GetValue(model));
            }
            return result;
        }

        /// <summary>
        /// 反射实现两个类的对象之间相同属性的值的复制
        /// 适用于初始化新实体(适用于建立实体的时候从一个实体做为数据源赋值app)
        /// </summary>
        /// <typeparam name="D">返回的实体</typeparam>
        /// <typeparam name="S">数据源实体</typeparam>
        /// <param name="s">数据源实体</param>
        /// <returns>返回的新实体</returns>
        public static D Mapper<D, S>(S s)
        {
            D d = Activator.CreateInstance<D>(); //构造新实例
            try
            {
                var Types = s.GetType();//得到类型  
                var Typed = typeof(D);
                foreach (PropertyInfo sp in Types.GetProperties())//得到类型的属性字段  
                {
                    foreach (PropertyInfo dp in Typed.GetProperties())
                    {
                        if (sp.Name == "Guid" && dp.Name == "ID" && dp.PropertyType == sp.PropertyType)
                        {
                            dp.SetValue(d, sp.GetValue(s, null), null);//得到s对象属性的值复制给d对象的属性
                        }
                        if (dp.Name == sp.Name && dp.PropertyType == sp.PropertyType && dp.Name != "Error" && dp.Name != "Item")//判断属性名是否相同  
                        {
                            dp.SetValue(d, sp.GetValue(s, null), null);//得到s对象属性的值复制给d对象的属性  
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return d;
        }

        /// <summary>
        /// 反射实现两个类的对象之间相同属性的值的复制
        /// 适用于没有新建实体之间(适用于没有建立实体，两个实体之间数据的转换)
        /// </summary>
        /// <typeparam name="D">返回的实体</typeparam>
        /// <typeparam name="S">数据源实体</typeparam>
        /// <param name="d">返回的实体</param>
        /// <param name="s">数据源实体</param>
        /// <returns></returns>
        public static D MapperToModel<D, S>(D d, S s)
        {
            try
            {
                var Types = s.GetType();//得到类型  
                var Typed = typeof(D);
                foreach (PropertyInfo sp in Types.GetProperties())//得到类型的属性字段  
                {
                    foreach (PropertyInfo dp in Typed.GetProperties())
                    {
                        if (dp.Name == sp.Name && dp.PropertyType == sp.PropertyType && dp.Name != "Error" && dp.Name != "Item")//判断属性名是否相同  
                        {
                            dp.SetValue(d, sp.GetValue(s, null), null);//得到s对象属性的值复制给d对象的属性  
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return d;
        }

        /// <summary>
        /// List<S>转List<T>
        /// </summary>
        /// <typeparam name="T">目标实体</typeparam>
        /// <typeparam name="S">数据源实体</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<T> ListToList<T, S>(List<S> source)
        {
            List<T> list = new List<T>();
            foreach (S data in source)
            {
                T obj = Mapper<T, S>(data);
                list.Add(obj);
            }
            return list;
        }

        #region 反射 类转换类、类集合转换类集合
        /// <summary>
        /// 类转类(T是接收的类型，A是传入数据的类型)
        /// 注意，不可以转换集合，转换集合请用SetListValueByListT方法
        /// 当前类用来转换实体类对象，只是属性和属性的赋值转换
        /// 只转换名字相同并且类型一致的
        /// </summary>
        /// <typeparam name="T">转换结果类型</typeparam>
        /// <typeparam name="A">要转换的类型</typeparam>
        /// <param name="GetValue">传入的参数</param>
        /// <returns>转换结果</returns>
        public static T SetValueByT<T, A>(A GetValue)
        {
            var SetValue = Activator.CreateInstance<T>();
            try
            {
                var TProps = SetValue.GetType().GetProperties();
                var TPropsNames = TProps.ToDictionary(i => i.Name, i => i.GetType());
                var TPropsTypes = TProps.Select(i => i.GetType()).ToList();

                var AProps = GetValue.GetType().GetProperties();

                List<PropertyInfo> Props = new List<PropertyInfo>();
                foreach (var item in AProps)
                {
                    if (TPropsNames.Keys.Contains(item.Name))
                    {
                        if (TPropsNames[item.Name] == item.GetType())
                        {
                            var Value = GetValue.GetType().GetProperty(item.Name).GetValue(GetValue);
                            SetValue.GetType().GetProperty(item.Name).SetValue(SetValue, Value);
                        }
                    }
                }
                return SetValue;
            }
            catch
            {
                return SetValue;
            }
        }

        /// <summary>
        /// 集合转换(T是接收的集合类型，A是传入数据的集合类型)
        /// 当前类用来转换实体类对象，只是属性和属性的赋值转换
        /// 只转换名字相同并且类型一致的
        /// </summary>
        /// <typeparam name="T">转换结果类型</typeparam>
        /// <typeparam name="A">要转换的类型</typeparam>
        /// <param name="GetListValue">传入的集合</param>
        /// <returns>转换结果</returns>
        public static List<T> SetListValueByListT<T, A>(List<A> GetListValue)
        {
            List<T> SetListValue = new List<T>();
            if (GetListValue.Count() > 0)
            {
                var SetDefaultValue = Activator.CreateInstance<T>();
                var TProps = SetDefaultValue.GetType().GetProperties();
                var TPropsNames = TProps.ToDictionary(i => i.Name, i => i.GetType());
                var TPropsTypes = TProps.Select(i => i.GetType()).ToList();
                var AProps = GetListValue.First().GetType().GetProperties();

                Dictionary<string, List<object>> list = new Dictionary<string, List<object>>();
                foreach (var item in AProps)
                {
                    if (TPropsNames.Keys.Contains(item.Name))
                    {
                        if (TPropsNames[item.Name] == item.GetType())
                        {
                            List<object> getPropList = GetListValue.Select(i => i.GetType().GetProperty(item.Name).GetValue(i)).ToList();
                            list.Add(item.Name, getPropList);
                        }
                    }
                }
                if (list.Keys.Count > 0)
                {
                    var Count = list.ElementAt(0).Value.Count;
                    for (int i = 0; i < Count; i++)
                    {
                        var NewValue = Activator.CreateInstance<T>();
                        foreach (var item in list)
                        {
                            NewValue.GetType().GetProperty(item.Key).SetValue(NewValue, item.Value.ElementAt(i));
                        }
                        SetListValue.Add(NewValue);
                    }
                }
            }
            return SetListValue;
        }
        #endregion
        #endregion


    }

    /// <summary>
    /// 高性能对象转换
    /// 1.采用静态泛型类缓存，避免了拆箱装箱操作。
    /// 2.对于转换对象中有，字段名一样但是类型不一样的类时仍可以用
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    public static class Mapper<TSource, TTarget> where TSource : class where TTarget : class
    {
        public readonly static Func<TSource, TTarget> Map;

        static Mapper()
        {
            if (Map == null)
                Map = GetMap();
        }

        private static Func<TSource, TTarget> GetMap()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            var parameterExpression = Expression.Parameter(sourceType, "p");
            var memberInitExpression = GetExpression(parameterExpression, sourceType, targetType);
            var lambda = Expression.Lambda<Func<TSource, TTarget>>(memberInitExpression, parameterExpression);
            return lambda.Compile();
        }

        /// <summary>
        /// 根据转换源和目标获取表达式树
        /// </summary>
        /// <param name="parameterExpression">表达式参数p</param>
        /// <param name="sourceType">转换源类型</param>
        /// <param name="targetType">转换目标类型</param>
        /// <returns></returns>
        private static MemberInitExpression GetExpression(Expression parameterExpression, Type sourceType, Type targetType)
        {
            var memberBindings = new List<MemberBinding>();
            foreach (var targetItem in targetType.GetProperties().Where(x => x.PropertyType.IsPublic && x.CanWrite))
            {
                var sourceItem = sourceType.GetProperty(targetItem.Name);
                //判断实体的读写权限
                if (sourceItem == null || !sourceItem.CanRead || sourceItem.PropertyType.IsNotPublic)
                    continue;
                //标注NotMapped特性的属性忽略转换
                if (sourceItem.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;
                var propertyExpression = Expression.Property(parameterExpression, sourceItem);
                //判断都是class且类型不相同时
                if (targetItem.PropertyType.IsClass && sourceItem.PropertyType.IsClass && targetItem.PropertyType != sourceItem.PropertyType)
                {
                    if (targetItem.PropertyType != targetType)//防止出现自己引用自己无限递归
                    {
                        var memberInit = GetExpression(propertyExpression, sourceItem.PropertyType, targetItem.PropertyType);
                        memberBindings.Add(Expression.Bind(targetItem, memberInit));
                        continue;
                    }
                }
                if (targetItem.PropertyType != sourceItem.PropertyType)
                    continue;
                memberBindings.Add(Expression.Bind(targetItem, propertyExpression));
            }
            return Expression.MemberInit(Expression.New(targetType), memberBindings);
        }
    }

}
