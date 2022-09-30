using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Reflection;
using MongoDB.Driver.GridFS;

namespace Shipeng.Util.Cache
{
    /// <summary>
    /// Mongo db的数据库帮助类
    /// Author:李仕鹏
    /// </summary>
    public class MongoDbHelper
    {
        /// <summary>  
        /// 数据库的实例  
        /// </summary>  
        private MongoDatabase _db;

        /// <summary>  
        /// ObjectId的键  
        /// </summary>  
        private readonly string OBJECTID_KEY = "_id";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host">数据库所在主机</param>
        /// <param name="port">数据库所在主机的端口,默认27017</param>
        /// <param name="db">连接对应的数据库名称--MongoDB需要指定连接的数据库</param>
        /// <param name="timeOut">连接超时设置 秒</param>        
        public MongoDbHelper(string host, int port= 27017, string db="mongdb",string timeOut="6000")
        {
            _db = new MongoDb(host, port,db, timeOut).GetDataBase();
        }

        public MongoDbHelper(MongoDatabase db)
        {
            _db = db;
        }

        public T GetNextSequence<T>(IMongoQuery query, SortByDocument sortBy,
            UpdateDocument update, string collectionName, string indexName)
        {
            if (_db == null)
            {
                return default(T);
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                query = InitQuery(query);
                sortBy = InitSortBy(sortBy, OBJECTID_KEY);
                update = InitUpdateDocument(update, indexName);
                var ido = mc.FindAndModify(query, sortBy, update, true, false);

                return ido.GetModifiedDocumentAs<T>();
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        #region 插入数据  
        /// <summary>  
        /// 将数据插入进数据库  
        /// </summary>  
        /// <typeparam name="T">需要插入数据的类型</typeparam>  
        /// <param name="t">需要插入的具体实体</param>  
        public bool Insert<T>(T t)
        {
            //集合名称  
            string collectionName = typeof(T).Name;
            return Insert(t, collectionName);
        }

        /// <summary>  
        /// 将数据插入进数据库  
        /// </summary>  
        /// <typeparam name="T">需要插入数据库的实体类型</typeparam>  
        /// <param name="t">需要插入数据库的具体实体</param>  
        /// <param name="collectionName">指定插入的集合</param>  
        public bool Insert<T>(T t, string collectionName)
        {
            if (_db == null)
            {
                return false;
            }
            try
            {
                MongoCollection<BsonDocument> mc = _db.GetCollection<BsonDocument>(collectionName);
                //将实体转换为bson文档  
                BsonDocument bd = t.ToBsonDocument();
                //进行插入操作  
                WriteConcernResult result = mc.Insert(bd);
                if (!string.IsNullOrEmpty(result.LastErrorMessage))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【Insert】将数据插入进数据库发生异常：{ex.Message}");
            }
        }

        /// <summary>  
        /// 批量插入数据  
        /// </summary>  
        /// <typeparam name="T">需要插入数据库的实体类型</typeparam>  
        /// <param name="list">需要插入数据的列表</param>  
        public bool Insert<T>(List<T> list)
        {
            //集合名称  
            string collectionName = typeof(T).Name;
            return Insert(list, collectionName);
        }

        /// <summary>  
        /// 批量插入数据  
        /// </summary>  
        /// <typeparam name="T">需要插入数据库的实体类型</typeparam>  
        /// <param name="list">需要插入数据的列表</param>  
        /// <param name="collectionName">指定要插入的集合</param>  
        public bool Insert<T>(List<T> list, string collectionName)
        {
            if (_db == null)
            {
                return false;
            }
            try
            {
                MongoCollection<BsonDocument> mc = _db.GetCollection<BsonDocument>(collectionName);
                //创建一个空间bson集合  
                List<BsonDocument> bsonList = new List<BsonDocument>();
                //批量将数据转为bson格式 并且放进bson文档  
                list.ForEach(t => bsonList.Add(t.ToBsonDocument()));
                //批量插入数据  
                mc.InsertBatch(bsonList);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【Insert】批量插入数据发生异常：{ex.Message}");
            }
        }
        #endregion

        #region 查询数据  

        #region 查询所有记录  
        /// <summary>  
        /// 查询一个集合中的所有数据  
        /// </summary>  
        /// <typeparam name="T">该集合数据的所属类型</typeparam>  
        /// <param name="collectionName">指定集合的名称</param>  
        /// <returns>返回一个List列表</returns>  
        public List<T> FindAll<T>(string collectionName)
        {
            if (_db == null)
            {
                return null;
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                //以实体方式取出其数据集合  
                MongoCursor<T> mongoCursor = mc.FindAll();
                //直接转化为List返回  
                return mongoCursor.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【FindAll】查询一个集合中的所有数据发生异常：{ex.Message}");
            }
        }

        /// <summary>  
        /// 查询一个集合中的所有数据 其集合的名称为T的名称  
        /// </summary>  
        /// <typeparam name="T">该集合数据的所属类型</typeparam>  
        /// <returns>返回一个List列表</returns>  
        public List<T> FindAll<T>()
        {
            string collectionName = typeof(T).Name;
            return FindAll<T>(collectionName);
        }
        #endregion

        #region 查询一条记录  
        /// <summary>  
        /// 查询索引最大的一条记录  
        /// </summary>  
        /// <typeparam name="T">该数据所属的类型</typeparam>  
        /// <param name="collectionName">去指定查询的集合</param>  
        /// <param name="sort">排序字段</param>  
        /// <returns>返回一个实体类型</returns>  
        public T FindOneToIndexMax<T>(string collectionName, string[] sort)
        {
            return FindOneToIndexMax<T>(null, collectionName, sort);
        }

        /// <summary>  
        /// 查询索引最大的一条记录  
        /// </summary>  
        /// <typeparam name="T">该数据所属的类型</typeparam>  
        /// <param name="query">查询的条件 可以为空</param>  
        /// <param name="collectionName">去指定查询的集合</param>  
        /// <param name="sort">排序字段</param>  
        /// <returns>返回一个实体类型</returns>  
        public T FindOneToIndexMax<T>(IMongoQuery query, string collectionName, string[] sort)
        {
            if (_db == null)
            {
                return default(T);
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                query = InitQuery(query);
                T t = mc.Find(query).SetSortOrder(SortBy.Descending(sort)).FirstOrDefault<T>();
                return t;
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【FindOneToIndexMax】查询索引最大的一条记录发生异常：{ex.Message}");
            }
        }
      
        /// <summary>  
        /// 查询一条记录  
        /// </summary>  
        /// <typeparam name="T">该数据所属的类型</typeparam>  
        /// <param name="query">查询的条件 可以为空</param>  
        /// <param name="collectionName">去指定查询的集合</param>  
        /// <returns>返回一个实体类型</returns>  
        public T FindOne<T>(IMongoQuery query, string collectionName)
        {
            if (_db == null)
            {
                return default(T);
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                query = InitQuery(query);
                T t = mc.FindOne(query);
                return t;
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【FindOne】查询一条记录发生异常：{ex.Message}");
            }
        }

        /// <summary>  
        /// 查询一条记录  
        /// </summary>  
        /// <typeparam name="T">该数据所属的类型</typeparam>  
        /// <param name="collectionName">去指定查询的集合</param>  
        /// <returns>返回一个实体类型</returns>  
        public T FindOne<T>(string collectionName)
        {
            return FindOne<T>(null, collectionName);
        }

        /// <summary>  
        /// 查询一条记录  
        /// </summary>  
        /// <typeparam name="T">该数据所属的类型</typeparam>  
        /// <returns>返回一个实体类型</returns>  
        public T FindOne<T>()
        {
            string collectionName = typeof(T).Name;
            return FindOne<T>(null, collectionName);
        }

        /// <summary>  
        /// 查询一条记录  
        /// </summary>  
        /// <typeparam name="T">该数据所属的类型</typeparam>  
        /// <param name="query">查询的条件 可以为空</param>  
        /// <returns>返回一个实体类型</returns>  
        public T FindOne<T>(IMongoQuery query)
        {
            string collectionName = typeof(T).Name;
            return FindOne<T>(query, collectionName);
        }
        #endregion

        #region 普通的条件查询  
        /// <summary>  
        /// 根据指定条件查询集合中的数据  
        /// </summary>  
        /// <typeparam name="T">该集合数据的所属类型</typeparam>  
        /// <param name="query">指定的查询条件 比如Query.And(Query.EQ("username","admin"),Query.EQ("password":"admin"))</param>  
        /// <param name="collectionName">指定的集合的名称</param>  
        /// <returns>返回一个List列表</returns>  
        public List<T> Find<T>(IMongoQuery query, string collectionName)
        {
            if (_db == null)
            {
                return null;
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                query = InitQuery(query);
                MongoCursor<T> mongoCursor = mc.Find(query);
                return mongoCursor.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【Find】根据指定条件查询集合中的数据发生异常：{ex.Message}");
            }
        }

        /// <summary>  
        /// 根据指定条件查询集合中的数据  
        /// </summary>  
        /// <typeparam name="T">该集合数据的所属类型</typeparam>  
        /// <param name="query">指定的查询条件 比如Query.And(Query.EQ("username","admin"),Query.EQ("password":"admin"))</param>  
        /// <returns>返回一个List列表</returns>  
        public List<T> Find<T>(IMongoQuery query)
        {
            string collectionName = typeof(T).Name;
            return Find<T>(query, collectionName);
        }
        #endregion

        #region 分页查询 PageIndex和PageSize模式  在页数PageIndex大的情况下 效率明显变低  
        /// <summary>  
        /// 分页查询 PageIndex和PageSize模式  在页数PageIndex大的情况下 效率明显变低  
        /// </summary>  
        /// <typeparam name="T">所需查询的数据的实体类型</typeparam>  
        /// <param name="query">查询的条件</param>  
        /// <param name="pageIndex">当前的页数</param>  
        /// <param name="pageSize">当前的尺寸</param>  
        /// <param name="sortBy">排序方式</param>  
        /// <param name="collectionName">集合名称</param>  
        /// <returns>返回List列表</returns>  
        public List<T> Find<T>(IMongoQuery query, int pageIndex, int pageSize, SortByDocument sortBy, string collectionName)
        {
            if (_db == null)
            {
                return null;
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                MongoCursor<T> mongoCursor = null;
                query = InitQuery(query);
                sortBy = InitSortBy(sortBy, OBJECTID_KEY);
                //如页序号为0时初始化为1  
                pageIndex = pageIndex == 0 ? 1 : pageIndex;
                //按条件查询 排序 跳数 取数  
                mongoCursor = mc.Find(query).SetSortOrder(sortBy).SetSkip((pageIndex - 1) * pageSize).SetLimit(pageSize);
                return mongoCursor.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【Find】分页查询PageIndex和PageSize发生异常：{ex.Message}");
            }
        }

        /// <summary>  
        /// 分页查询 PageIndex和PageSize模式  在页数PageIndex大的情况下 效率明显变低  
        /// </summary>  
        /// <typeparam name="T">所需查询的数据的实体类型</typeparam>  
        /// <param name="query">查询的条件</param>  
        /// <param name="pageIndex">当前的页数</param>  
        /// <param name="pageSize">当前的尺寸</param>  
        /// <param name="sortBy">排序方式</param>  
        /// <returns>返回List列表</returns>  
        public List<T> Find<T>(IMongoQuery query, int pageIndex, int pageSize, SortByDocument sortBy)
        {
            string collectionName = typeof(T).Name;
            return Find<T>(query, pageIndex, pageSize, sortBy, collectionName);
        }

        #endregion

        #region 分页查询 指定索引最后项-PageSize模式  
        /// <summary>  
        /// 分页查询 指定索引最后项-PageSize模式   
        /// </summary>  
        /// <typeparam name="T">所需查询的数据的实体类型</typeparam>  
        /// <param name="query">查询的条件 没有可以为null</param>  
        /// <param name="indexName">索引名称</param>  
        /// <param name="lastKeyValue">最后索引的值</param>  
        /// <param name="pageSize">分页的尺寸</param>  
        /// <param name="sortType">排序类型 1升序 -1降序 仅仅针对该索引</param>  
        /// <param name="collectionName">指定的集合名称</param>  
        /// <returns>返回一个List列表数据</returns>  
        public List<T> Find<T>(IMongoQuery query, string indexName, object lastKeyValue, int pageSize, int sortType, string collectionName)
        {
            if (_db == null)
            {
                return null;
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                MongoCursor<T> mongoCursor = null;
                query = InitQuery(query);

                //判断升降序后进行查询  
                if (sortType > 0)
                {
                    //升序  
                    if (lastKeyValue != null)
                    {
                        //有上一个主键的值传进来时才添加上一个主键的值的条件  
                        query = Query.And(query, Query.GT(indexName, BsonValue.Create(lastKeyValue)));
                    }
                    //先按条件查询 再排序 再取数  
                    mongoCursor = mc.Find(query).SetSortOrder(new SortByDocument(indexName, 1)).SetLimit(pageSize);
                }
                else
                {
                    //降序  
                    if (lastKeyValue != null)
                    {
                        query = Query.And(query, Query.LT(indexName, BsonValue.Create(lastKeyValue)));
                    }
                    mongoCursor = mc.Find(query).SetSortOrder(new SortByDocument(indexName, -1)).SetLimit(pageSize);
                }
                return mongoCursor.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【Find】分页查询指定索引最后项-PageSize模式发生异常：{ex.Message}");
            }
        }

        /// <summary>  
        /// 分页查询 指定索引最后项-PageSize模式   
        /// </summary>  
        /// <typeparam name="T">所需查询的数据的实体类型</typeparam>  
        /// <param name="query">查询的条件 没有可以为null</param>  
        /// <param name="indexName">索引名称</param>  
        /// <param name="lastKeyValue">最后索引的值</param>  
        /// <param name="pageSize">分页的尺寸</param>  
        /// <param name="sortType">排序类型 1升序 -1降序 仅仅针对该索引</param>  
        /// <returns>返回一个List列表数据</returns>  
        public List<T> Find<T>(IMongoQuery query, string indexName, object lastKeyValue, int pageSize, int sortType)
        {
            string collectionName = typeof(T).Name;
            return Find<T>(query, indexName, lastKeyValue, pageSize, sortType, collectionName);
        }

        /// <summary>  
        /// 分页查询 指定ObjectId最后项-PageSize模式   
        /// </summary>  
        /// <typeparam name="T">所需查询的数据的实体类型</typeparam>  
        /// <param name="query">查询的条件 没有可以为null</param>  
        /// <param name="lastObjectId">上一条记录的ObjectId 没有可以为空</param>  
        /// <param name="pageSize">每页尺寸</param>  
        /// <param name="sortType">排序类型 1升序 -1降序 仅仅针对_id</param>  
        /// <param name="collectionName">指定去查询集合的名称</param>  
        /// <returns>返回一个List列表数据</returns>  
        public List<T> Find<T>(IMongoQuery query, string lastObjectId, int pageSize, int sortType, string collectionName)
        {
            return Find<T>(query, OBJECTID_KEY, new ObjectId(lastObjectId), pageSize, sortType, collectionName);
        }

        /// <summary>  
        /// 分页查询 指定ObjectId最后项-PageSize模式   
        /// </summary>  
        /// <typeparam name="T">所需查询的数据的实体类型</typeparam>  
        /// <param name="query">查询的条件 没有可以为null</param>  
        /// <param name="lastObjectId">上一条记录的ObjectId 没有可以为空</param>  
        /// <param name="pageSize">每页尺寸</param>  
        /// <param name="sortType">排序类型 1升序 -1降序 仅仅针对_id</param>  
        /// <returns>返回一个List列表数据</returns>  
        public List<T> Find<T>(IMongoQuery query, string lastObjectId, int pageSize, int sortType)
        {
            string collectionName = typeof(T).Name;
            return Find<T>(query, lastObjectId, pageSize, sortType, collectionName);
        }

        #endregion

        #endregion

        #region 更新数据  
        /// <summary>  
        /// 更新数据  
        /// </summary>  
        /// <typeparam name="T">更新的数据 所属的类型</typeparam>  
        /// <param name="query">更新数据的查询</param>  
        /// <param name="update">需要更新的文档</param>  
        /// <param name="collectionName">指定更新集合的名称</param>  
        public bool Update<T>(IMongoQuery query, IMongoUpdate update, string collectionName)
        {
            if (_db == null)
            {
                return false;
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                query = InitQuery(query);
                //更新数据  
                WriteConcernResult result = mc.Update(query, update, UpdateFlags.Multi);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【Update】更新数据发生异常：{ex.Message}");
            }
        }

        /// <summary>  
        /// 更新数据  
        /// </summary>  
        /// <typeparam name="T">更新的数据 所属的类型</typeparam>  
        /// <param name="query">更新数据的查询</param>  
        /// <param name="update">需要更新的文档</param>  
        public bool Update<T>(IMongoQuery query, IMongoUpdate update)
        {
            string collectionName = typeof(T).Name;
            return Update<T>(query, update, collectionName);
        }

        #endregion

        #region 移除/删除数据  
        /// <summary>  
        /// 移除指定的数据  
        /// </summary>  
        /// <typeparam name="T">移除的数据类型</typeparam>  
        /// <param name="query">移除的数据条件</param>  
        /// <param name="collectionName">指定的集合名词</param>  
        public bool Remove<T>(IMongoQuery query, string collectionName)
        {
            if (_db == null)
            {
                return false;
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                query = InitQuery(query);
                //根据指定查询移除数据  
                mc.Remove(query);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【Remove】移除指定的数据发生异常：{ex.Message}");
            }
        }

        /// <summary>  
        /// 移除指定的数据  
        /// </summary>  
        /// <typeparam name="T">移除的数据类型</typeparam>  
        /// <param name="query">移除的数据条件</param>  
        public bool Remove<T>(IMongoQuery query)
        {
            string collectionName = typeof(T).Name;
            return Remove<T>(query, collectionName);
        }

        /// <summary>  
        /// 移除实体里面所有的数据  
        /// </summary>  
        /// <typeparam name="T">移除的数据类型</typeparam>  
        public bool ReomveAll<T>()
        {
            string collectionName = typeof(T).Name;
            return Remove<T>(null, collectionName);
        }

        /// <summary>  
        /// 移除实体里面所有的数据  
        /// </summary>  
        /// <typeparam name="T">移除的数据类型</typeparam>  
        /// <param name="collectionName">指定的集合名称</param>  
        public bool RemoveAll<T>(string collectionName)
        {
            return Remove<T>(null, collectionName);
        }
        #endregion

        #region 创建索引  
        /// <summary>  
        /// 创建索引   
        /// </summary>  
        /// <typeparam name="T">需要创建索引的实体类型</typeparam>  
        public bool CreateIndex<T>()
        {
            if (_db == null)
            {
                return false;
            }
            try
            {
                string collectionName = typeof(T).Name;
                MongoCollection<BsonDocument> mc = _db.GetCollection<BsonDocument>(collectionName);

                PropertyInfo[] propertys = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
                //得到该实体类型的属性  
                foreach (PropertyInfo property in propertys)
                {
                    //在各个属性中得到其特性  
                    foreach (object obj in property.GetCustomAttributes(true))
                    {
                        MongoDbFieldAttribute mongoField = obj as MongoDbFieldAttribute;
                        if (mongoField != null)
                        {// 此特性为mongodb的字段属性  

                            IndexKeysBuilder indexKey;
                            if (mongoField.Ascending)
                            {
                                //升序 索引  
                                indexKey = IndexKeys.Ascending(property.Name);
                            }
                            else
                            {
                                //降序索引  
                                indexKey = IndexKeys.Descending(property.Name);
                            }
                            //创建该属性  
                            mc.CreateIndex(indexKey, IndexOptions.SetUnique(mongoField.Unique));
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【CreateIndex】创建索引发生异常：{ex.Message}");
            }
        }
        #endregion

        #region 获取表的行数  
        /// <summary>  
        /// 获取数据表总行数  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="query"></param>  
        /// <param name="collectionName"></param>  
        /// <returns></returns>  
        public long GetCount<T>(IMongoQuery query, string collectionName)
        {
            if (_db == null)
            {
                return 0;
            }
            try
            {
                MongoCollection<T> mc = _db.GetCollection<T>(collectionName);
                if (query == null)
                {
                    return mc.Count();
                }
                else
                {
                    return mc.Count(query);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【GetCount】获取数据表总行数发生异常：{ex.Message}");
            }
        }
      
        /// <summary>  
        /// 获取数据表总行数  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="query"></param>  
        /// <returns></returns>  
        public long GetCount<T>(IMongoQuery query)
        {
            string collectionName = typeof(T).Name;
            return GetCount<T>(query, collectionName);
        }

        #endregion

        #region 获取集合的存储大小  
        /// <summary>  
        /// 获取集合的存储大小  
        /// </summary>  
        /// <typeparam name="T">该集合对应的实体类</typeparam>  
        /// <returns>返回一个long型</returns>  
        public long GetDataSize<T>()
        {
            string collectionName = typeof(T).Name;
            return GetDataSize(collectionName);
        }

        /// <summary>  
        /// 获取集合的存储大小  
        /// </summary>  
        /// <param name="collectionName">该集合对应的名称</param>  
        /// <returns>返回一个long型</returns>  
        public long GetDataSize(string collectionName)
        {
            if (_db == null)
            {
                return 0;
            }
            try
            {
                MongoCollection<BsonDocument> mc = _db.GetCollection<BsonDocument>(collectionName);
                //return mc.GetTotalStorageSize();
                //return mc.Count();//集合中的文档数量
                return mc.GetStats().StorageSize;//存储大小
            }
            catch (Exception ex)
            {
                throw new Exception($"MongoDbHelper【GetDataSize】获取集合的存储大小发生异常：{ex.Message}");
            }
        }
        #endregion

        #region 私有的一些辅助方法  
        /// <summary>  
        /// 初始化查询记录 主要当该查询条件为空时 会附加一个恒真的查询条件，防止空查询报错  
        /// </summary>  
        /// <param name="query">查询的条件</param>  
        /// <returns></returns>  
        private IMongoQuery InitQuery(IMongoQuery query)
        {
            if (query == null)
            {
                //当查询为空时 附加恒真的条件 类似SQL：1=1的语法  
                query = Query.Exists(OBJECTID_KEY);
            }
            return query;
        }

        /// <summary>  
        /// 初始化排序条件  主要当条件为空时 会默认以ObjectId递增的一个排序  
        /// </summary>  
        /// <param name="sortBy"></param>  
        /// <param name="sortByName"></param>  
        /// <returns></returns>  
        private SortByDocument InitSortBy(SortByDocument sortBy, string sortByName)
        {
            if (sortBy == null)
            {
                //默认ObjectId 递增  
                sortBy = new SortByDocument(sortByName, -1);
            }
            return sortBy;
        }

        private UpdateDocument InitUpdateDocument(UpdateDocument update, string indexName)
        {
            if (update == null)
            {
                update = new UpdateDocument("$inc", new QueryDocument(indexName, 0));
            }
            return update;
        }
        #endregion

        #region 通过mongoGridS保存文件
        /// <summary>
        /// 通过mongoGridS保存文件
        /// </summary>
        /// <param name="FileData">文件二进制流</param>
        /// <param name="FileName">文件名称</param>
        public bool SaveFilesByGridFS(byte[] FileData, string FileName,
            BsonDocument doc, string FilesType)
        {
            bool flag = false;
            try
            {
                MongoGridFSSettings fsSetting = new MongoGridFSSettings() { Root = FilesType };
                MongoGridFS fs = new MongoGridFS(_db, fsSetting);

                //通过Metadata 添加附加信息
                MongoGridFSCreateOptions option = new MongoGridFSCreateOptions();
                option.UploadDate = DateTime.Now;

                //BsonDocument doc = new BsonDocument();
                //doc.Add("UserID", 1L);
                //doc.Add("GUID", System.Guid.NewGuid());
                option.Metadata = doc;

                //创建文件，文件并存储数据
                using (MongoGridFSStream gfs = fs.Create(FileName, option))
                {
                    gfs.Write(FileData, 0, FileData.Length);
                    gfs.Close();
                }
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
                throw new Exception($"MongoDbHelper【SaveFilesByGridFS】通过mongoGridS保存文件发生异常：{ex.Message}");
            }
            return flag;

        }
        #endregion

        #region 通过mongoGridS得到图片
        /// <summary>
        /// 通过mongoGridS得到图片
        /// </summary>
        /// <param name="FileData">文件二进制流</param>
        /// <param name="FileName">文件名称</param>
        public byte[] GetFileByGridFS(string ResGuid, string FilesType)
        {
            byte[] bytes;
            try
            {
                MongoGridFSSettings fsSetting = new MongoGridFSSettings() { Root = FilesType };
                MongoGridFS gridfs = new MongoGridFS(_db, fsSetting);
                // 定义查询条件
                var query = new QueryDocument();
                var b = new BsonDocument();
                b.Add("metadata.GUID", ResGuid);
                query.Add(b);
                //得到文件信息
                MongoGridFSFileInfo VideoInfo = gridfs.FindOne(query);
                if (VideoInfo != null)
                {
                    //读取
                    using (MongoGridFSStream gridFileStream = VideoInfo.OpenRead())
                    {
                        //数组大小
                        bytes = new byte[gridFileStream.Length];
                        //存入二进制
                        gridFileStream.Read(bytes, 0, (int)gridFileStream.Length);

                        //   gridfs.Download(_Stream, query, 1);

                    }
                }
                else
                {
                    bytes = null;
                }
            }
            catch (Exception ex)
            {
                bytes = null;
                throw new Exception($"MongoDbHelper【GetFileByGridFS】通过mongoGridS得到图片发生异常：{ex.Message}");
            }
            return bytes;
        }
        #endregion

        #region 通过ResGuid,删除图片
        /// <summary>
        /// 通过ResGuid,删除图片
        /// </summary>
        /// <param name="FileData">文件二进制流</param>
        /// <param name="FileName">文件名称</param>
        public bool DelFileByGridFS(string ResGuid, string FilesType)
        {
            bool flag = false;
            try
            {
                MongoGridFSSettings fsSetting = new MongoGridFSSettings() { Root = FilesType };
                MongoGridFS gridfs = new MongoGridFS(_db, fsSetting);
                // 定义查询条件
                var query = new QueryDocument();
                var b = new BsonDocument();
                b.Add("metadata.GUID", ResGuid);
                query.Add(b);
                //得到文件信息
                MongoGridFSFileInfo ExistImage = gridfs.FindOne(query);
                if (ExistImage != null)
                {
                    ExistImage.Delete();

                    flag = true;
                }
                else
                {
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                flag = false;
                throw new Exception($"MongoDbHelper【DelFileByGridFS】通过ResGuid,删除图片发生异常：{ex.Message}");
            }
            return flag;
        }
        #endregion

    }

}

