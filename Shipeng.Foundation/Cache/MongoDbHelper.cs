using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Reflection;

namespace Shipeng.Util.Cache
{
    /// <summary>
    /// MongoDB 常用操作帮助类。
    /// </summary>
    /// <remarks>
    /// 本类已迁移到 MongoDB.Driver 3.x 的新版 API。旧版驱动中的 MongoDatabase、MongoCollection、
    /// IMongoQuery、SortByDocument、UpdateDocument、MongoGridFS 等类型已经被移除，继续使用会导致
    /// .NET 10 和最新版 NuGet 无法编译。新版实现统一使用 IMongoDatabase、FilterDefinition、
    /// SortDefinition、UpdateDefinition 和 GridFSBucket，减少过时 API 风险。
    /// </remarks>
    public class MongoDbHelper
    {
        private const string ObjectIdKey = "_id";
        private readonly IMongoDatabase _db;

        /// <summary>
        /// 根据连接信息创建 MongoDB 帮助类。
        /// </summary>
        /// <param name="host">MongoDB 服务所在主机。</param>
        /// <param name="port">MongoDB 服务端口，默认 27017。</param>
        /// <param name="db">MongoDB 数据库名称。</param>
        /// <param name="timeOut">连接超时时间，单位为秒。</param>
        public MongoDbHelper(string host, int port = 27017, string db = "mongdb", string timeOut = "6000")
        {
            _db = new MongoDb(host, port, db, timeOut).GetDataBase();
        }

        /// <summary>
        /// 使用外部已创建的数据库实例初始化帮助类，便于复用同一个 MongoClient 连接池。
        /// </summary>
        /// <param name="db">新版 MongoDB 数据库实例。</param>
        public MongoDbHelper(IMongoDatabase db)
        {
            _db = db;
        }

        /// <summary>
        /// 查找并更新一条序列记录，返回更新后的文档。
        /// </summary>
        /// <typeparam name="T">集合文档类型。</typeparam>
        /// <param name="query">查询条件；为空时匹配全部文档。</param>
        /// <param name="sortBy">排序条件；为空时按 _id 降序。</param>
        /// <param name="update">更新表达式；为空时默认对 indexName 字段执行 +1。</param>
        /// <param name="collectionName">集合名称。</param>
        /// <param name="indexName">序列字段名。</param>
        /// <returns>更新后的文档；未匹配到数据时返回默认值。</returns>
        public T GetNextSequence<T>(
            FilterDefinition<T> query,
            SortDefinition<T> sortBy,
            UpdateDefinition<T> update,
            string collectionName,
            string indexName)
        {
            if (_db == null)
            {
                return default;
            }

            try
            {
                var collection = _db.GetCollection<T>(collectionName);
                var options = new FindOneAndUpdateOptions<T>
                {
                    Sort = InitSortBy(sortBy),
                    ReturnDocument = ReturnDocument.After
                };

                return collection.FindOneAndUpdate(
                    InitQuery(query),
                    update ?? Builders<T>.Update.Inc(indexName, 1),
                    options);
            }
            catch
            {
                return default;
            }
        }

        #region 插入数据

        /// <summary>
        /// 将单条实体插入到以类型名命名的集合中。
        /// </summary>
        public bool Insert<T>(T t)
        {
            return Insert(t, typeof(T).Name);
        }

        /// <summary>
        /// 将单条实体插入到指定集合中。
        /// </summary>
        public bool Insert<T>(T t, string collectionName)
        {
            if (_db == null || t == null)
            {
                return false;
            }

            try
            {
                _db.GetCollection<T>(collectionName).InsertOne(t);
                return true;
            }
            catch (Exception ex)
            {
                throw CreateException("Insert", "将数据插入进数据库", ex);
            }
        }

        /// <summary>
        /// 将实体列表批量插入到以类型名命名的集合中。
        /// </summary>
        public bool Insert<T>(List<T> list)
        {
            return Insert(list, typeof(T).Name);
        }

        /// <summary>
        /// 将实体列表批量插入到指定集合中。
        /// </summary>
        /// <remarks>
        /// 新版驱动的 InsertMany 会直接使用驱动序列化管线，避免旧实现先整体转换成 BsonDocument 再写入，
        /// 在大批量写入时能减少一次额外的内存分配。
        /// </remarks>
        public bool Insert<T>(List<T> list, string collectionName)
        {
            if (_db == null || list == null || list.Count == 0)
            {
                return false;
            }

            try
            {
                _db.GetCollection<T>(collectionName).InsertMany(list);
                return true;
            }
            catch (Exception ex)
            {
                throw CreateException("Insert", "批量插入数据", ex);
            }
        }

        #endregion

        #region 查询数据

        /// <summary>
        /// 查询指定集合的全部记录。
        /// </summary>
        public List<T> FindAll<T>(string collectionName)
        {
            if (_db == null)
            {
                return null;
            }

            try
            {
                return _db.GetCollection<T>(collectionName)
                    .Find(Builders<T>.Filter.Empty)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw CreateException("FindAll", "查询一个集合中的所有数据", ex);
            }
        }

        /// <summary>
        /// 查询以类型名命名的集合中的全部记录。
        /// </summary>
        public List<T> FindAll<T>()
        {
            return FindAll<T>(typeof(T).Name);
        }

        /// <summary>
        /// 按指定字段降序查询第一条记录。
        /// </summary>
        public T FindOneToIndexMax<T>(string collectionName, string[] sort)
        {
            return FindOneToIndexMax<T>(null, collectionName, sort);
        }

        /// <summary>
        /// 按指定字段降序查询第一条记录。
        /// </summary>
        public T FindOneToIndexMax<T>(FilterDefinition<T> query, string collectionName, string[] sort)
        {
            if (_db == null)
            {
                return default;
            }

            try
            {
                return _db.GetCollection<T>(collectionName)
                    .Find(InitQuery(query))
                    .Sort(BuildDescendingSort<T>(sort))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw CreateException("FindOneToIndexMax", "查询索引最大的一条记录", ex);
            }
        }

        /// <summary>
        /// 按条件查询一条记录。
        /// </summary>
        public T FindOne<T>(FilterDefinition<T> query, string collectionName)
        {
            if (_db == null)
            {
                return default;
            }

            try
            {
                return _db.GetCollection<T>(collectionName)
                    .Find(InitQuery(query))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw CreateException("FindOne", "查询一条记录", ex);
            }
        }

        /// <summary>
        /// 查询指定集合中的第一条记录。
        /// </summary>
        public T FindOne<T>(string collectionName)
        {
            return FindOne<T>(null, collectionName);
        }

        /// <summary>
        /// 查询以类型名命名的集合中的第一条记录。
        /// </summary>
        public T FindOne<T>()
        {
            return FindOne<T>(null, typeof(T).Name);
        }

        /// <summary>
        /// 按条件查询以类型名命名的集合中的第一条记录。
        /// </summary>
        public T FindOne<T>(FilterDefinition<T> query)
        {
            return FindOne<T>(query, typeof(T).Name);
        }

        /// <summary>
        /// 按条件查询指定集合中的全部匹配记录。
        /// </summary>
        public List<T> Find<T>(FilterDefinition<T> query, string collectionName)
        {
            if (_db == null)
            {
                return null;
            }

            try
            {
                return _db.GetCollection<T>(collectionName)
                    .Find(InitQuery(query))
                    .ToList();
            }
            catch (Exception ex)
            {
                throw CreateException("Find", "根据指定条件查询集合中的数据", ex);
            }
        }

        /// <summary>
        /// 按条件查询以类型名命名的集合中的全部匹配记录。
        /// </summary>
        public List<T> Find<T>(FilterDefinition<T> query)
        {
            return Find<T>(query, typeof(T).Name);
        }

        /// <summary>
        /// 使用 pageIndex/pageSize 方式分页查询。
        /// </summary>
        /// <remarks>
        /// Skip 在页码很大时会让 MongoDB 扫描并丢弃大量文档；保留该方法是为了兼容老调用。
        /// 性能敏感场景应优先使用“最后索引值 + pageSize”的游标式分页方法。
        /// </remarks>
        public List<T> Find<T>(
            FilterDefinition<T> query,
            int pageIndex,
            int pageSize,
            SortDefinition<T> sortBy,
            string collectionName)
        {
            if (_db == null)
            {
                return null;
            }

            try
            {
                pageIndex = pageIndex <= 0 ? 1 : pageIndex;
                pageSize = pageSize <= 0 ? 20 : pageSize;

                return _db.GetCollection<T>(collectionName)
                    .Find(InitQuery(query))
                    .Sort(InitSortBy(sortBy))
                    .Skip((pageIndex - 1) * pageSize)
                    .Limit(pageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw CreateException("Find", "分页查询PageIndex和PageSize", ex);
            }
        }

        /// <summary>
        /// 使用 pageIndex/pageSize 方式分页查询以类型名命名的集合。
        /// </summary>
        public List<T> Find<T>(FilterDefinition<T> query, int pageIndex, int pageSize, SortDefinition<T> sortBy)
        {
            return Find<T>(query, pageIndex, pageSize, sortBy, typeof(T).Name);
        }

        /// <summary>
        /// 使用最后索引值进行游标式分页查询。
        /// </summary>
        /// <remarks>
        /// 该方法通过索引范围条件继续向后取数，避免大页码 Skip 的线性扫描成本。
        /// </remarks>
        public List<T> Find<T>(
            FilterDefinition<T> query,
            string indexName,
            object lastKeyValue,
            int pageSize,
            int sortType,
            string collectionName)
        {
            if (_db == null)
            {
                return null;
            }

            try
            {
                pageSize = pageSize <= 0 ? 20 : pageSize;
                var builder = Builders<T>.Filter;
                var filter = InitQuery(query);

                if (lastKeyValue != null)
                {
                    var rangeFilter = sortType > 0
                        ? builder.Gt(indexName, BsonValue.Create(lastKeyValue))
                        : builder.Lt(indexName, BsonValue.Create(lastKeyValue));
                    filter = builder.And(filter, rangeFilter);
                }

                var sort = sortType > 0
                    ? Builders<T>.Sort.Ascending(indexName)
                    : Builders<T>.Sort.Descending(indexName);

                return _db.GetCollection<T>(collectionName)
                    .Find(filter)
                    .Sort(sort)
                    .Limit(pageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw CreateException("Find", "分页查询指定索引最后项-PageSize模式", ex);
            }
        }

        /// <summary>
        /// 使用最后索引值进行游标式分页查询以类型名命名的集合。
        /// </summary>
        public List<T> Find<T>(FilterDefinition<T> query, string indexName, object lastKeyValue, int pageSize, int sortType)
        {
            return Find<T>(query, indexName, lastKeyValue, pageSize, sortType, typeof(T).Name);
        }

        /// <summary>
        /// 使用上一条 _id 进行游标式分页查询。
        /// </summary>
        public List<T> Find<T>(FilterDefinition<T> query, string lastObjectId, int pageSize, int sortType, string collectionName)
        {
            object lastKey = string.IsNullOrWhiteSpace(lastObjectId) ? null : ObjectId.Parse(lastObjectId);
            return Find<T>(query, ObjectIdKey, lastKey, pageSize, sortType, collectionName);
        }

        /// <summary>
        /// 使用上一条 _id 进行游标式分页查询以类型名命名的集合。
        /// </summary>
        public List<T> Find<T>(FilterDefinition<T> query, string lastObjectId, int pageSize, int sortType)
        {
            return Find<T>(query, lastObjectId, pageSize, sortType, typeof(T).Name);
        }

        #endregion

        #region 更新数据

        /// <summary>
        /// 按条件批量更新指定集合中的数据。
        /// </summary>
        public bool Update<T>(FilterDefinition<T> query, UpdateDefinition<T> update, string collectionName)
        {
            if (_db == null || update == null)
            {
                return false;
            }

            try
            {
                _db.GetCollection<T>(collectionName).UpdateMany(InitQuery(query), update);
                return true;
            }
            catch (Exception ex)
            {
                throw CreateException("Update", "更新数据", ex);
            }
        }

        /// <summary>
        /// 按条件批量更新以类型名命名的集合。
        /// </summary>
        public bool Update<T>(FilterDefinition<T> query, UpdateDefinition<T> update)
        {
            return Update<T>(query, update, typeof(T).Name);
        }

        #endregion

        #region 移除/删除数据

        /// <summary>
        /// 按条件删除指定集合中的数据。
        /// </summary>
        public bool Remove<T>(FilterDefinition<T> query, string collectionName)
        {
            if (_db == null)
            {
                return false;
            }

            try
            {
                _db.GetCollection<T>(collectionName).DeleteMany(InitQuery(query));
                return true;
            }
            catch (Exception ex)
            {
                throw CreateException("Remove", "移除指定的数据", ex);
            }
        }

        /// <summary>
        /// 按条件删除以类型名命名的集合中的数据。
        /// </summary>
        public bool Remove<T>(FilterDefinition<T> query)
        {
            return Remove<T>(query, typeof(T).Name);
        }

        /// <summary>
        /// 删除以类型名命名的集合中的全部数据。
        /// </summary>
        public bool ReomveAll<T>()
        {
            return Remove<T>(null, typeof(T).Name);
        }

        /// <summary>
        /// 删除指定集合中的全部数据。
        /// </summary>
        public bool RemoveAll<T>(string collectionName)
        {
            return Remove<T>(null, collectionName);
        }

        #endregion

        #region 创建索引

        /// <summary>
        /// 根据实体属性上的 <see cref="MongoDbFieldAttribute"/> 创建索引。
        /// </summary>
        public bool CreateIndex<T>()
        {
            if (_db == null)
            {
                return false;
            }

            try
            {
                var collection = _db.GetCollection<BsonDocument>(typeof(T).Name);
                var propertys = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                foreach (PropertyInfo property in propertys)
                {
                    var mongoField = property.GetCustomAttributes(true).OfType<MongoDbFieldAttribute>().FirstOrDefault();
                    if (mongoField == null || !mongoField.IsIndex)
                    {
                        continue;
                    }

                    var keys = mongoField.Ascending
                        ? Builders<BsonDocument>.IndexKeys.Ascending(property.Name)
                        : Builders<BsonDocument>.IndexKeys.Descending(property.Name);
                    var options = new CreateIndexOptions { Unique = mongoField.Unique };

                    collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(keys, options));
                }

                return true;
            }
            catch (Exception ex)
            {
                throw CreateException("CreateIndex", "创建索引", ex);
            }
        }

        #endregion

        #region 统计信息

        /// <summary>
        /// 获取指定集合中符合条件的文档总数。
        /// </summary>
        public long GetCount<T>(FilterDefinition<T> query, string collectionName)
        {
            if (_db == null)
            {
                return 0;
            }

            try
            {
                return _db.GetCollection<T>(collectionName).CountDocuments(InitQuery(query));
            }
            catch (Exception ex)
            {
                throw CreateException("GetCount", "获取数据表总行数", ex);
            }
        }

        /// <summary>
        /// 获取以类型名命名的集合中符合条件的文档总数。
        /// </summary>
        public long GetCount<T>(FilterDefinition<T> query)
        {
            return GetCount<T>(query, typeof(T).Name);
        }

        /// <summary>
        /// 获取以类型名命名的集合存储大小。
        /// </summary>
        public long GetDataSize<T>()
        {
            return GetDataSize(typeof(T).Name);
        }

        /// <summary>
        /// 获取指定集合存储大小，单位为字节。
        /// </summary>
        public long GetDataSize(string collectionName)
        {
            if (_db == null)
            {
                return 0;
            }

            try
            {
                var stats = _db.RunCommand<BsonDocument>(new BsonDocument("collStats", collectionName));
                return stats.TryGetValue("storageSize", out BsonValue storageSize) ? storageSize.ToInt64() : 0;
            }
            catch (Exception ex)
            {
                throw CreateException("GetDataSize", "获取集合的存储大小", ex);
            }
        }

        #endregion

        #region GridFS 文件操作

        /// <summary>
        /// 通过 GridFS 保存文件。
        /// </summary>
        /// <param name="FileData">文件二进制内容。</param>
        /// <param name="FileName">文件名称。</param>
        /// <param name="doc">文件元数据。</param>
        /// <param name="FilesType">GridFS bucket 名称。</param>
        public bool SaveFilesByGridFS(byte[] FileData, string FileName, BsonDocument doc, string FilesType)
        {
            if (_db == null || FileData == null || FileData.Length == 0)
            {
                return false;
            }

            try
            {
                var bucket = CreateBucket(FilesType);
                var options = new GridFSUploadOptions
                {
                    Metadata = doc
                };

                bucket.UploadFromBytes(FileName, FileData, options);
                return true;
            }
            catch (Exception ex)
            {
                throw CreateException("SaveFilesByGridFS", "通过mongoGridS保存文件", ex);
            }
        }

        /// <summary>
        /// 通过 GridFS 元数据 GUID 读取文件内容。
        /// </summary>
        /// <param name="ResGuid">元数据中的 GUID。</param>
        /// <param name="FilesType">GridFS bucket 名称。</param>
        public byte[] GetFileByGridFS(string ResGuid, string FilesType)
        {
            if (_db == null || string.IsNullOrWhiteSpace(ResGuid))
            {
                return null;
            }

            try
            {
                var bucket = CreateBucket(FilesType);
                var fileInfo = bucket.Find(Builders<GridFSFileInfo>.Filter.Eq("metadata.GUID", ResGuid)).FirstOrDefault();
                return fileInfo == null ? null : bucket.DownloadAsBytes(fileInfo.Id);
            }
            catch (Exception ex)
            {
                throw CreateException("GetFileByGridFS", "通过mongoGridS得到图片", ex);
            }
        }

        /// <summary>
        /// 通过 GridFS 元数据 GUID 删除文件。
        /// </summary>
        /// <param name="ResGuid">元数据中的 GUID。</param>
        /// <param name="FilesType">GridFS bucket 名称。</param>
        public bool DelFileByGridFS(string ResGuid, string FilesType)
        {
            if (_db == null || string.IsNullOrWhiteSpace(ResGuid))
            {
                return false;
            }

            try
            {
                var bucket = CreateBucket(FilesType);
                var fileInfo = bucket.Find(Builders<GridFSFileInfo>.Filter.Eq("metadata.GUID", ResGuid)).FirstOrDefault();
                if (fileInfo == null)
                {
                    return false;
                }

                bucket.Delete(fileInfo.Id);
                return true;
            }
            catch (Exception ex)
            {
                throw CreateException("DelFileByGridFS", "通过ResGuid,删除图片", ex);
            }
        }

        #endregion

        #region 私有辅助方法

        private FilterDefinition<T> InitQuery<T>(FilterDefinition<T> query)
        {
            return query ?? Builders<T>.Filter.Empty;
        }

        private SortDefinition<T> InitSortBy<T>(SortDefinition<T> sortBy)
        {
            return sortBy ?? Builders<T>.Sort.Descending(ObjectIdKey);
        }

        private SortDefinition<T> BuildDescendingSort<T>(string[] sort)
        {
            if (sort == null || sort.Length == 0)
            {
                return Builders<T>.Sort.Descending(ObjectIdKey);
            }

            var sortBuilder = Builders<T>.Sort;
            SortDefinition<T> result = null;
            foreach (string field in sort.Where(static item => !string.IsNullOrWhiteSpace(item)))
            {
                result = result == null
                    ? sortBuilder.Descending(field)
                    : sortBuilder.Combine(result, sortBuilder.Descending(field));
            }

            return result ?? sortBuilder.Descending(ObjectIdKey);
        }

        private GridFSBucket CreateBucket(string filesType)
        {
            return new GridFSBucket(_db, new GridFSBucketOptions
            {
                BucketName = string.IsNullOrWhiteSpace(filesType) ? "fs" : filesType
            });
        }

        private static Exception CreateException(string methodName, string action, Exception ex)
        {
            return new Exception($"MongoDbHelper【{methodName}】{action}发生异常：{ex.Message}", ex);
        }

        #endregion
    }
}
