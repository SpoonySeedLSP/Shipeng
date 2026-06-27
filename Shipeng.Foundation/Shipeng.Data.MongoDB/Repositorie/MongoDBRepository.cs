using MongoDB.Bson;
using MongoDB.Driver;
using Shipeng.Util.Cache;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Shipeng.Data.MongoDB.Repositorie
{
    /// <summary>
    /// MongoDB仓储实现类
    /// </summary>
    public partial class MongoDBRepository<TEntity> : IMongoDBRepository<TEntity>
    where TEntity : class, new()
    {
        private readonly TraceDbMongoDBContext _traceDbMongoDBContext;//mongodb上下文
        private readonly IMongoDatabase db;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="traceDbMongoDBContext"></param>
        public MongoDBRepository(TraceDbMongoDBContext traceDbMongoDBContext)
        {
            _traceDbMongoDBContext = traceDbMongoDBContext;
            //从MongoDB上下文获取或者创建放置对应信息的库(sql中的库)
            //数据库不存在，也没有关系，它会在首次使用数据库的时候进行自动创建
            db = _traceDbMongoDBContext.Database;
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        /// <param name="collectionName">数据集</param>
        /// <returns></returns>
        public bool Exists(string collectionName)
        {
            var options = new ListCollectionsOptions
            {
                Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName)
            };
            return db.ListCollections(options).ToEnumerable().Any();
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        /// <param name="collectionName">数据集</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string collectionName)
        {
            var options = new ListCollectionsOptions
            {
                Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName)
            };
            return (await db.ListCollectionsAsync(options)).ToEnumerable().Any();
        }

        /// <summary>  
        /// 创建索引   
        /// </summary>  
        public bool CreateIndex()
        {
            try
            {
                string collectionName = typeof(TEntity).Name;
                IMongoCollection<BsonDocument> mc = db.GetCollection<BsonDocument>(collectionName);

                PropertyInfo[] propertys = typeof(TEntity).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
                //得到该实体类型的属性  
                foreach (PropertyInfo property in propertys)
                {
                    //在各个属性中得到其特性  
                    foreach (object obj in property.GetCustomAttributes(true))
                    {
                        MongoDbFieldAttribute? mongoField = obj as MongoDbFieldAttribute;
                        if (mongoField != null)
                        {// 此特性为mongodb的字段属性  
                            IndexKeysDefinition<BsonDocument> indexKey;
                            if (mongoField.Ascending)
                            {
                                //升序 索引  
                                indexKey = Builders<BsonDocument>.IndexKeys.Ascending(property.Name);
                            }
                            else
                            {
                                //降序索引  
                                indexKey = Builders<BsonDocument>.IndexKeys.Descending(property.Name);
                            }
                            //创建该属性  
                            mc.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(indexKey, new CreateIndexOptions { Unique = mongoField.Unique }));
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        public bool Add(TEntity entity,string name)
        {
            try
            {
                /* 获取或者创建线路跟踪集合
                 * 2、我们可以调用database的GetCollection<TDocument> 方法来获取数据集，(sql中的表)
                 * 2-1其中如果数据是预先定义好的可以在<输入数据的类型>  ,如果是没有定义好的，
                 * 可以使用BsonDocument类型，BsonDocument表示没有预定于的模式。
                 * 2-2我们将获取到上面“db”所对应的数据库中的“freight_line”集合，
                 * 即使“freight_line”集合不存在也没有关系，
                 * 同数据库一样，若数据集不存在，会自动创建该数据集。
                 */
                var client = db.GetCollection<TEntity>(name);
                client.InsertOne(entity);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步添加一条数据
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        public async Task<bool> AddAsync(TEntity entity, string name)
        {
            try
            {
                foreach (var item in entity.GetType().GetProperties())
                {
                    if ((item.PropertyType == typeof(DateTime) || item.PropertyType == typeof(DateTime?))
                        && item.GetValue(entity) != null)
                    {
                        //mongodb插入日期格式的数据时发现，日期时间相差8个小时，
                        //原来存储在mongodb中的时间是标准时间UTC +0:00，而中国的时区是+8.00
                        //因此在插入的时候需要对时间进行处理：
                        //DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                        item.SetValue(entity, DateTime.SpecifyKind((DateTime)item.GetValue(entity), DateTimeKind.Utc));
                    }
                }
                var client = db.GetCollection<TEntity>(name);
                await client.InsertOneAsync(entity);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>  
        /// 将数据插入进数据库  
        /// </summary>  
        /// <param name="entity">需要插入数据库的具体实体</param>  
        /// <param name="name">指定插入的集合</param>  
        public bool Insert(TEntity entity, string name)
        {
            try
            {
                IMongoCollection<BsonDocument> mc = db.GetCollection<BsonDocument>(name);
                //将实体转换为bson文档  
                BsonDocument bd = entity.ToBsonDocument();
                //进行插入操作  
                mc.InsertOne(bd);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        public bool AddList(List<TEntity> entities, string name)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                client.InsertMany(entities);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步批量插入
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        public async Task<bool> AddListAsync(List<TEntity> entities, string name)
        {
            try
            {
                if (!entities.Any()) return false;
                foreach (TEntity entity in entities)
                {
                    foreach (var item in entity.GetType().GetProperties())
                    {
                        if ((item.PropertyType == typeof(DateTime) || item.PropertyType == typeof(DateTime?))
                         && item.GetValue(entity) != null)
                        {
                            //mongodb插入日期格式的数据时发现，日期时间相差8个小时，
                            //原来存储在mongodb中的时间是标准时间UTC +0:00，而中国的时区是+8.00
                            //因此在插入的时候需要对时间进行处理：
                            //DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                            item.SetValue(entity, DateTime.SpecifyKind((DateTime)item.GetValue(entity), DateTimeKind.Utc));
                        }
                    }
                }
                var client = db.GetCollection<TEntity>(name);
                await client.InsertManyAsync(entities);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步批量插入海量数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        public async Task<bool> InsertManyAsync(List<TEntity> entities, string name)
        {
            try
            {
                if (!entities.Any()) return false;              
                var stopwatch = Stopwatch.StartNew();
                stopwatch.Start();
                //使用AsParallel()方法来利用并行处理进行优化，以减少处理时间
                entities.AsParallel().ForAll(entity => {
                    entity.GetType().GetProperties().AsParallel().ForAll(item => {
                        if ((item.PropertyType == typeof(DateTime) || item.PropertyType == typeof(DateTime?)) && item.GetValue(entity) != null)
                        {
                            item.SetValue(entity, DateTime.SpecifyKind((DateTime)item.GetValue(entity), DateTimeKind.Utc));
                        }
                    });
                });
                stopwatch.Stop();
                var client = db.GetCollection<TEntity>(name);
                await client.InsertManyAsync(entities);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>  
        /// 批量插入数据  
        /// </summary>  
        /// <param name="list">需要插入数据的列表</param>  
        /// <param name="name">指定要插入的集合</param>  
        public bool InsertBatch(List<TEntity> list, string name)
        {
            try
            {
                IMongoCollection<BsonDocument> mc = db.GetCollection<BsonDocument>(name);
                //创建一个空间bson集合  
                List<BsonDocument> bsonList = new List<BsonDocument>();
                //批量将数据转为bson格式 并且放进bson文档  
                list.ForEach(t => bsonList.Add(t.ToBsonDocument()));
                //批量插入数据  
                mc.InsertMany(bsonList);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public UpdateResult Update(TEntity entity, string name, string id)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                //修改条件
                FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq("_id", new ObjectId(id));
                //要修改的字段
                var list = new List<UpdateDefinition<TEntity>>();
                foreach (var item in entity.GetType().GetProperties())
                {
                    if (item.Name.ToLower() == "id") continue;
                    list.Add(Builders<TEntity>.Update.Set(item.Name, item.GetValue(entity)));
                }
                var updatefilter = Builders<TEntity>.Update.Combine(list);
                return client.UpdateOne(filter, updatefilter);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步修改一条数据
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdateAsync(TEntity entity, string name, string id)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                //修改条件
                FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq("_id", new ObjectId(id));
                //要修改的字段
                var list = new List<UpdateDefinition<TEntity>>();
                foreach (var item in entity.GetType().GetProperties())
                {
                    if (item.Name.ToLower() == "id") continue;
                    list.Add(Builders<TEntity>.Update.Set(item.Name, item.GetValue(entity)));
                }
                var updatefilter = Builders<TEntity>.Update.Combine(list);
                return await client.UpdateOneAsync(filter, updatefilter);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 更新 MongoDB 数据。
        /// 集合或记录不存在时自动新增。
        /// 为兼容业务层大量使用 ModifiedCount > 0 判断成功的写法，
        /// 匹配但内容未变化、自动新增成功、无字段可更新但记录存在时，统一返回 ModifiedCount=1。
        /// </summary>
        /// <param name="entity">更新实体</param>
        /// <param name="name">集合名称</param>
        /// <param name="filter">更新条件</param>
        /// <returns>更新结果</returns>
        public async Task<UpdateResult> UpdateAsync(TEntity entity, string name, Expression<Func<TEntity, bool>> filter)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity), "MongoDB更新实体不能为空");
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name), "MongoDB集合名称不能为空");

                var client = db.GetCollection<TEntity>(name);
                var updates = new List<UpdateDefinition<TEntity>>();

                foreach (var property in entity.GetType().GetProperties())
                {
                    if (!property.CanRead) continue;
                    if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) continue;

                    var value = property.GetValue(entity);

                    // MongoDB 按 UTC 保存时间，避免保存后与北京时间相差 8 小时。
                    if ((property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?)) && value != null)
                        value = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);

                    updates.Add(Builders<TEntity>.Update.Set(property.Name, value));
                }

                // 没有可更新字段时，只保证数据存在。
                if (!updates.Any())
                {
                    if (await GetAsync(name, filter) == null)
                        await AddAsync(entity, name);

                    return new UpdateResult.Acknowledged(1, 1, null);
                }

                var update = Builders<TEntity>.Update.Combine(updates);
                var result = await client.UpdateOneAsync(filter, update);

                // MongoDB 中缺少该记录时自动补新增。
                if (result.MatchedCount <= 0)
                {
                    await AddAsync(entity, name);
                    return new UpdateResult.Acknowledged(1, 1, null);
                }

                // 匹配到了但内容没变化，也视为保存成功。
                if (result.ModifiedCount <= 0)
                    return new UpdateResult.Acknowledged(result.MatchedCount, 1, result.UpsertedId);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 更新 MongoDB 数据。
        /// 如果集合不存在，或者当前条件没有匹配到记录，则自动新增当前实体。
        /// 
        /// 注意：
        /// 1. 该方法不是“纯更新”语义，而是“更新优先、没有则新增”的保存语义。
        /// 2. 如果业务上必须严格要求“只能更新，不能新增”，请单独写 PureUpdateAsync。
        /// 3. MongoDB 的 ModifiedCount 表示实际修改数量，内容完全一样时可能为 0，不应作为失败依据。
        /// 4. 判断是否匹配到旧数据，应使用 MatchedCount。
        /// </summary>
        /// <param name="entity">需要保存到 MongoDB 的实体对象</param>
        /// <param name="name">MongoDB 集合名称</param>
        /// <param name="filter">更新条件，用于匹配需要更新的 MongoDB 文档</param>
        /// <returns>
        /// UpdateResult：
        /// MatchedCount > 0 表示匹配到了旧数据；
        /// ModifiedCount > 0 表示旧数据内容确实发生变化；
        /// 如果自动新增成功，则返回一个已确认的 Acknowledged 结果。
        /// </returns>
        public async Task<UpdateResult> UpdateOneAsync(TEntity entity, string name, Expression<Func<TEntity, bool>> filter)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity), "MongoDB更新实体不能为空");

                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name), "MongoDB集合名称不能为空");

                // 获取集合对象。
                // MongoDB 驱动的 GetCollection 不要求集合必须提前存在。
                // 如果集合不存在，后续 AddAsync 插入数据时 MongoDB 会自动创建集合。
                var client = db.GetCollection<TEntity>(name);

                // 保存需要更新的字段集合。
                // 这里通过反射读取实体属性，把除 Id 外的属性全部构造成 Set 更新。
                var updateDefinitions = new List<UpdateDefinition<TEntity>>();

                foreach (var property in entity.GetType().GetProperties())
                {
                    // 跳过不可读属性，避免反射读取时报错。
                    if (!property.CanRead) continue;

                    // 跳过 Id 字段。
                    // MongoDB 的 _id 不允许通过 $set 修改，否则会报错。
                    if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) continue;

                    var value = property.GetValue(entity);

                    // MongoDB 内部按 UTC 保存 DateTime。
                    // 如果不指定 Kind，保存后可能出现与中国时区相差 8 小时的问题。
                    if ((property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?)) && value != null)
                    {
                        value = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
                    }

                    updateDefinitions.Add(Builders<TEntity>.Update.Set(property.Name, value));
                }

                // 没有任何可更新字段时，不执行 UpdateOne。
                // 这种情况通常说明实体只有 Id，或者所有属性都被跳过。
                // 此时如果记录不存在则新增；如果记录存在则返回已确认结果。
                if (!updateDefinitions.Any())
                {
                    var exists = await GetAsync(name, filter);

                    if (exists == null)
                    {
                        await AddAsync(entity, name);

                        // 自动新增成功，返回一个“已确认”的结果，方便业务层统一判断。
                        return new UpdateResult.Acknowledged(1, 1, null);
                    }

                    // 记录存在，但没有字段需要更新。
                    // 这里返回 MatchedCount=1，ModifiedCount=0，表示匹配成功但没有内容变化。
                    return new UpdateResult.Acknowledged(1, 0, null);
                }

                var updateFilter = Builders<TEntity>.Update.Combine(updateDefinitions);

                // 先尝试更新。
                // 这样可以少一次“先查再更新”的数据库往返，性能比先 GetAsync 再 UpdateOne 更好。
                var result = await client.UpdateOneAsync(filter, updateFilter);

                // MatchedCount <= 0 表示没有匹配到旧文档。
                // 这种情况通常是 MongoDB 同步数据缺失，所以自动补新增。
                if (result.MatchedCount <= 0)
                {
                    await AddAsync(entity, name);

                    // 自动新增成功，返回一个“已确认”的结果。
                    return new UpdateResult.Acknowledged(1, 1, null);
                }

                // MatchedCount > 0 表示更新条件命中了数据。
                // 即使 ModifiedCount = 0，也说明保存请求是有效的，只是内容没有变化。
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步保存一条数据（存在则更新，不存在则插入，原子操作，无并发问题）
        /// 这是 MongoDB 官方推荐的方式，单条请求完成「判断+更新/插入」，彻底避免多线程/高并发场景下的竞态问题
        /// result.UpsertedId != null 表示插入成功，result.ModifiedCount > 0 表示更新成功
        /// </summary>
        /// <param name="entity">要保存的实体数据</param>
        /// <param name="name">MongoDB集合名称</param>
        /// <param name="filter">匹配条件（建议使用唯一条件，如_id或业务主键）</param>
        /// <returns>操作结果（可判断是新增还是更新）</returns>
        public async Task<UpdateResult> SaveAsync(TEntity entity, string name, Expression<Func<TEntity, bool>> filter)
        {
            try
            {
                // 验证实体不能为空，避免后面反射读取属性时报空引用
                if (entity == null) throw new ArgumentNullException(nameof(entity), "MongoDB保存实体不能为空");

                // 验证集合名称不能为空，避免获取MongoDB集合时报错
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name), "MongoDB集合名称不能为空");

                // 验证匹配条件不能为空，Upsert必须有明确的匹配条件
                if (filter == null) throw new ArgumentNullException(nameof(filter), "MongoDB保存条件不能为空");

                // 1. 获取目标集合（MongoDB会自动创建不存在的集合，无需手动判断）
                var collection = db.GetCollection<TEntity>(name);

                // 2. 构建更新定义：遍历实体属性，生成Set/SetOnInsert操作
                var updateDefs = new List<UpdateDefinition<TEntity>>();
                object? value = null;

                foreach (var prop in typeof(TEntity).GetProperties())
                {
                    // 跳过不可读属性，避免反射读取时报错
                    if (!prop.CanRead) continue;

                    // 获取属性值
                    value = prop.GetValue(entity);

                    // 主键字段不能更新，因为MongoDB不允许修改_id
                    // 但是新增时需要写入主键，所以使用SetOnInsert，只在插入时设置
                    if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value != null)
                            updateDefs.Add(Builders<TEntity>.Update.SetOnInsert(prop.Name, value));

                        continue;
                    }

                    // 跳过空值，避免用null覆盖数据库中已有的有效数据
                    if (value == null) continue;

                    // 处理DateTime类型：转为UTC时间存储，解决MongoDB 8小时时区差问题
                    if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    {
                        value = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
                    }

                    // 添加更新项：文档存在时更新该字段
                    updateDefs.Add(Builders<TEntity>.Update.Set(prop.Name, value));
                }

                // 如果没有任何可更新字段，说明实体中除了Id以外没有有效值
                // 这种情况下不能执行空Update，否则MongoDB会报错
                if (!updateDefs.Any())
                {
                    // 先判断当前记录是否存在
                    var exists = await GetAsync(name, filter);

                    // 当前记录不存在时，直接新增实体
                    if (exists == null)
                        await AddAsync(entity, name);

                    // 为兼容业务层 ModifiedCount > 0 的成功判断，统一返回 ModifiedCount=1
                    return new UpdateResult.Acknowledged(1, 1, null);
                }

                // 合并所有更新操作
                var update = Builders<TEntity>.Update.Combine(updateDefs);

                // 3. 执行原子Upsert操作：匹配filter的文档存在则更新，不存在则插入新文档
                var updateOptions = new UpdateOptions { IsUpsert = true };
                var result = await collection.UpdateOneAsync(filter, update, updateOptions);

                // UpsertedId != null 表示本次执行的是插入
                // 为兼容业务层 ModifiedCount > 0 的成功判断，插入成功时转换为 ModifiedCount=1
                if (result.UpsertedId != null)
                    return new UpdateResult.Acknowledged(result.MatchedCount, 1, result.UpsertedId);

                // MatchedCount > 0 且 ModifiedCount = 0，表示匹配到了旧数据，但内容没有变化
                // 这种情况属于保存成功，不应该让业务层误判为失败
                if (result.MatchedCount > 0 && result.ModifiedCount <= 0)
                    return new UpdateResult.Acknowledged(result.MatchedCount, 1, result.UpsertedId);

                // 正常返回MongoDB原始更新结果
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 替换单个文档
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<ReplaceOneResult> ReplaceOneAsync(TEntity entity, string name,
            Expression<Func<TEntity, bool>> filter)
        {
            try
            {
                //数据表不存在添加
                if (!await ExistsAsync(name)) await AddAsync(entity, name);
                //当前记录不存在添加
                if ((await GetAsync(name, filter)) == null) await AddAsync(entity, name);
                var client = db.GetCollection<TEntity>(name);
                return await client.ReplaceOneAsync(filter, entity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>  
        /// 更新数据  
        /// </summary>  
        /// <param name="query">更新数据的查询</param>  
        /// <param name="update">需要更新的文档</param>  
        /// <param name="name">指定更新集合的名称</param>  
        /// <param name="id">ObjectId的键</param>
        public bool Update(FilterDefinition<TEntity> query, UpdateDefinition<TEntity> update, string name, string id)
        {
            try
            {
                IMongoCollection<TEntity> mc = db.GetCollection<TEntity>(name);
                query = InitQuery(query, id);
                //更新数据  
                mc.UpdateMany(query, update);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 批量修改数据
        /// </summary>
        /// <param name="dic">要修改的字段</param>
        /// <param name="name">数据集</param>
        /// <param name="filter">修改条件</param>
        /// <returns></returns>
        public UpdateResult UpdateManay(Dictionary<string, string> dic, string name, FilterDefinition<TEntity> filter)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                TEntity entity = new TEntity();
                //要修改的字段
                var list = new List<UpdateDefinition<TEntity>>();
                foreach (var item in entity.GetType().GetProperties())
                {
                    if (!dic.ContainsKey(item.Name)) continue;
                    var value = dic[item.Name];
                    list.Add(Builders<TEntity>.Update.Set(item.Name, value));
                }
                var updatefilter = Builders<TEntity>.Update.Combine(list);
                return client.UpdateMany(filter, updatefilter);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步批量修改数据
        /// </summary>
        /// <param name="dic">要修改的字段</param>
        /// <param name="name">数据集</param>
        /// <param name="filter">修改条件</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdateManayAsync(Dictionary<string, string> dic, string name, FilterDefinition<TEntity> filter)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                TEntity entity = new TEntity();
                //要修改的字段
                var list = new List<UpdateDefinition<TEntity>>();
                foreach (var item in entity.GetType().GetProperties())
                {
                    if (!dic.ContainsKey(item.Name)) continue;
                    var value = dic[item.Name];
                    list.Add(Builders<TEntity>.Update.Set(item.Name, value));
                }
                var updatefilter = Builders<TEntity>.Update.Combine(list);
                return await client.UpdateManyAsync(filter, updatefilter);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 批量修改数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        public async Task<int> UpdateManayAsync(List<TEntity> entities, string name)
        {
            try
            {
                int result = 0;
                if (!await ExistsAsync(name))
                {
                    if (await AddListAsync(entities, name)) return entities.Count();
                }
                var client = db.GetCollection<TEntity>(name);
                //要修改的字段
                var list = new List<UpdateDefinition<TEntity>>();
                UpdateResult update;
                var id="";
                object? value = null;
                foreach (TEntity entity in entities)
                {                   
                    list = new List<UpdateDefinition<TEntity>>();
                    foreach (var item in entity.GetType().GetProperties())
                    {                     
                        if (item.Name.ToLower() == "id")
                        {
                            id = item.GetValue(entity).ToString();
                            continue;
                        }
                        value = item.GetValue(entity);
                        if ((item.PropertyType == typeof(DateTime) || item.PropertyType == typeof(DateTime?))
                        && value != null)
                        {
                            //mongodb插入日期格式的数据时发现，日期时间相差8个小时，
                            //原来存储在mongodb中的时间是标准时间UTC +0:00，而中国的时区是+8.00
                            //因此在插入的时候需要对时间进行处理：
                            //DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                            value = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
                        }
                        list.Add(Builders<TEntity>.Update.Set(item.Name, value));                 
                    }
                    UpdateDefinition<TEntity> updatefilter = Builders<TEntity>.Update.Combine(list);
                    var filter = Builders<TEntity>.Filter.Eq("_id", id);
                    update =await client.UpdateManyAsync(filter, updatefilter);
                    if (update.ModifiedCount > 0) result++;
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="id">objectId</param>
        /// <returns></returns>
        public DeleteResult Delete(string name, string id)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq("_id", new ObjectId(id));
                return client.DeleteOne(filter);
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 异步删除一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="id">objectId</param>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteAsync(string name, string id)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                //修改条件
                FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq("_id", new ObjectId(id));
                return await client.DeleteOneAsync(filter);
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>  
        /// 移除指定的数据  
        /// </summary>  
        /// <param name="query">移除的数据条件</param>  
        /// <param name="name">指定的集合名词</param>  
        public bool Remove(FilterDefinition<TEntity> query, string name)
        {
            try
            {
                IMongoCollection<TEntity> mc = db.GetCollection<TEntity>(name);
                query = InitQuery(query,"");
                //根据指定查询移除数据  
                mc.DeleteMany(query);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">删除的条件</param>
        /// <returns></returns>
        public DeleteResult DeleteMany(string name, FilterDefinition<TEntity> filter)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                return client.DeleteMany(filter);
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 根据条件删除数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="where">删除的条件</param>
        /// <returns></returns>
        public long Delete(string name, Expression<Func<TEntity, bool>> where)
        {
            try
            {
                if (!Exists(name)) return 1;
                var client = db.GetCollection<TEntity>(name);
                return client.DeleteMany(where).DeletedCount;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步删除多条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">删除的条件</param>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteManyAsync(string name, FilterDefinition<TEntity> filter)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                return await client.DeleteManyAsync(filter);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步删除多条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="where">删除的条件</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string name, Expression<Func<TEntity, bool>> where)
        {
            try
            {
                if (!await ExistsAsync(name)) return 1;
                var client = db.GetCollection<TEntity>(name);
                return (await client.DeleteManyAsync(where)).DeletedCount;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 根据id查询一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="id">objectid</param>
        /// <param name="field">要查询的字段，不写时查询全部</param>
        /// <returns></returns>
        public TEntity Get(string name, string id, string[]? field = null)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq("_id", new ObjectId(id));
                //不指定查询字段
                if (field == null || field.Length == 0)
                {
                    return client.Find(filter).FirstOrDefault<TEntity>();
                }

                //指定查询字段
                var fieldList = new List<ProjectionDefinition<TEntity>>();
                for (int i = 0; i < field.Length; i++)
                {
                    fieldList.Add(Builders<TEntity>.Projection.Include(field[i].ToString()));
                }
                var projection = Builders<TEntity>.Projection.Combine(fieldList);
                fieldList?.Clear();
                return client.Find(filter).Project<TEntity>(projection).FirstOrDefault<TEntity>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 根据条件获取一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="whereExpression">条件</param>
        /// <returns></returns>
        public TEntity Get(string name, Expression<Func<TEntity, bool>> whereExpression)
        {
            var client = db.GetCollection<TEntity>(name);
            var entity=client.Find(whereExpression).FirstOrDefault();
            return entity;
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(string name,Expression<Func<TEntity, bool>> whereExpression)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                var entity= (await client.FindAsync(whereExpression)).FirstOrDefaultAsync().Result;
                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步根据id查询一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="id">objectid</param>
        /// <param name="field">要查询的字段，不写时查询全部</param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(string name, string id, string[]? field = null)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq("_id", new ObjectId(id));
                //不指定查询字段
                if (field == null || field.Length == 0)
                {
                    return await client.Find(filter).FirstOrDefaultAsync();
                }

                //指定查询字段
                var fieldList = new List<ProjectionDefinition<TEntity>>();
                for (int i = 0; i < field.Length; i++)
                {
                    fieldList.Add(Builders<TEntity>.Projection.Include(field[i].ToString()));
                }
                var projection = Builders<TEntity>.Projection.Combine(fieldList);
                fieldList?.Clear();
                return await client.Find(filter).Project<TEntity>(projection).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public List<TEntity> GetList(string name, FilterDefinition<TEntity> filter, string[]? field = null, SortDefinition<TEntity>? sort = null)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                //不指定查询字段
                if (field == null || field.Length == 0)
                {
                    if (sort == null) return client.Find(filter).ToList();
                    //进行排序
                    return client.Find(filter).Sort(sort).ToList();
                }

                //指定查询字段
                var fieldList = new List<ProjectionDefinition<TEntity>>();
                for (int i = 0; i < field.Length; i++)
                {
                    fieldList.Add(Builders<TEntity>.Projection.Include(field[i].ToString()));
                }
                var projection = Builders<TEntity>.Projection.Combine(fieldList);
                fieldList?.Clear();
                if (sort == null) return client.Find(filter).Project<TEntity>(projection).ToList();
                //排序查询
                return client.Find(filter).Sort(sort).Project<TEntity>(projection).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> GetListAsync(string name, Expression<Func<TEntity, bool>> whereExpression)
        {
            try
            {
                var list = new List<TEntity>();
                if (!await ExistsAsync(name)) return list;
                var client = db.GetCollection<TEntity>(name);
                list = (await client.FindAsync(whereExpression)).ToListAsync().Result;
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 根据条件获取条数
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="whereExpression">条件</param>
        /// <returns></returns>
        public async Task<long> CountAsync(string name, Expression<Func<TEntity, bool>> whereExpression)
        {
            try
            {
                long count = 0;
                if (!await ExistsAsync(name)) return 0;
                var client = db.GetCollection<TEntity>(name);
                count = await client.CountDocumentsAsync(whereExpression);
                return count;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步查询集合
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public async Task<List<TEntity>> GetListAsync(string name, FilterDefinition<TEntity> filter, string[]? field = null, SortDefinition<TEntity>? sort = null)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                //不指定查询字段
                if (field == null || field.Length == 0)
                {
                    if (sort == null) return await client.Find(filter).ToListAsync();
                    return await client.Find(filter).Sort(sort).ToListAsync();
                }

                //指定查询字段
                var fieldList = new List<ProjectionDefinition<TEntity>>();
                for (int i = 0; i < field.Length; i++)
                {
                    fieldList.Add(Builders<TEntity>.Projection.Include(field[i].ToString()));
                }
                var projection = Builders<TEntity>.Projection.Combine(fieldList);
                fieldList?.Clear();
                if (sort == null) return await client.Find(filter).Project<TEntity>(projection).ToListAsync();
                //排序查询
                return await client.Find(filter).Sort(sort).Project<TEntity>(projection).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 分页查询集合
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">查询条件</param>
        /// <param name="pageIndex">页码，从1开始</param>
        /// <param name="pageSize">页数据量</param>
        /// <param name="count">总条数</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public List<TEntity> GetList(string name, FilterDefinition<TEntity> filter, int pageIndex, int pageSize, out long count, string[]? field = null, SortDefinition<TEntity>? sort = null)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                count = client.CountDocuments(filter);
                if (pageIndex < 1 || pageSize < 1)
                {
                    return null;
                }
                //不指定查询字段
                if (field == null || field.Length == 0)
                {
                    if (sort == null) return client.Find(filter).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
                    //进行排序
                    return client.Find(filter).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
                }

                //指定查询字段
                var fieldList = new List<ProjectionDefinition<TEntity>>();
                for (int i = 0; i < field.Length; i++)
                {
                    fieldList.Add(Builders<TEntity>.Projection.Include(field[i].ToString()));
                }
                var projection = Builders<TEntity>.Projection.Combine(fieldList);
                fieldList?.Clear();

                //不排序
                if (sort == null) return client.Find(filter).Project<TEntity>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();

                //排序查询
                return client.Find(filter).Sort(sort).Project<TEntity>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步分页查询集合
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">查询条件</param>
        /// <param name="pageIndex">页码，从1开始</param>
        /// <param name="pageSize">页数据量</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public async Task<List<TEntity>> GetListAsync(string name, FilterDefinition<TEntity> filter, int pageIndex, int pageSize, string[]? field = null, SortDefinition<TEntity>? sort = null)
        {
            try
            {
                var client = db.GetCollection<TEntity>(name);
                if (pageIndex < 1 || pageSize < 1)
                {
                    return null;
                }
                //不指定查询字段
                if (field == null || field.Length == 0)
                {
                    if (sort == null) return await client.Find(filter).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
                    //进行排序
                    return await client.Find(filter).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
                }

                //指定查询字段
                var fieldList = new List<ProjectionDefinition<TEntity>>();
                for (int i = 0; i < field.Length; i++)
                {
                    fieldList.Add(Builders<TEntity>.Projection.Include(field[i].ToString()));
                }
                var projection = Builders<TEntity>.Projection.Combine(fieldList);
                fieldList?.Clear();

                //不排序
                if (sort == null) return await client.Find(filter).Project<TEntity>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();

                //排序查询
                return await client.Find(filter).Sort(sort).Project<TEntity>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步分页查询集合
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="where">查询条件</param>
        /// <param name="pageIndex">页码，从1开始</param>
        /// <param name="pageSize">页数据量</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns>Item1.结果集 Item2.总条数 Item3.总页数</returns>
        public async Task<(List<TEntity>,int,int)> GetListAsync(string name, Expression<Func<TEntity, bool>> where,
            int pageIndex, int pageSize, string[]? field = null, SortDefinition<TEntity>? sort = null)
        {
            try
            {             
                List<TEntity> result=new List<TEntity>();int pageCount = 0;int totalCount = 0;
                if(!await ExistsAsync(name)) return (result, totalCount, pageCount);
                var client = db.GetCollection<TEntity>(name);
                if (pageIndex < 1 || pageSize < 1) return (result, totalCount, pageCount);

                totalCount = (int)await client.CountDocumentsAsync(where);
                //不指定查询字段
                if (field == null || field.Length == 0)
                {
                    if (sort==null)
                        result = await client.Find(where).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
                    //进行排序
                    else 
                        result= await client.Find(where).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
                }
                else
                {
                    //指定查询字段
                    var fieldList = new List<ProjectionDefinition<TEntity>>();
                    for (int i = 0; i < field.Length; i++)
                    {
                        fieldList.Add(Builders<TEntity>.Projection.Include(field[i].ToString()));
                    }
                    var projection = Builders<TEntity>.Projection.Combine(fieldList);
                    fieldList?.Clear();

                    //不排序
                    if (sort==null) 
                        result= await client.Find(where).Project<TEntity>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();

                    //排序查询
                    else
                        result = await client.Find(where).Sort(sort).Project<TEntity>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
                }
                pageCount = (int)Math.Ceiling(totalCount / (decimal)pageSize);
                return (result, totalCount, pageCount);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>  
        /// 初始化查询记录 主要当该查询条件为空时 会附加一个恒真的查询条件，防止空查询报错  
        /// </summary>  
        /// <param name="query">查询的条件</param>  
        /// <param name="id">ObjectId的键</param>
        /// <returns></returns>  
        private FilterDefinition<TEntity> InitQuery(FilterDefinition<TEntity> query,string id)
        {
            if (query == null)
            {
                //当查询为空时 附加恒真的条件 类似SQL：1=1的语法  
                query = string.IsNullOrWhiteSpace(id) ? Builders<TEntity>.Filter.Empty : Builders<TEntity>.Filter.Exists(id);
            }
            return query;
        }

    }
}
