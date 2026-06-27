using MongoDB.Driver;
using System.Linq.Expressions;

namespace Shipeng.Data.MongoDB.Repositorie
{
    /// <summary>
    /// MongoDB仓储接口
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial interface IMongoDBRepository<TEntity>
        where TEntity : class, new()
    {
        /// <summary>
        /// 检查表是否存在
        /// </summary>
        /// <param name="collectionName">数据集</param>
        /// <returns></returns>
        bool Exists(string collectionName);

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        /// <param name="collectionName">数据集</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string collectionName);

        /// <summary>  
        /// 创建索引   
        /// </summary>  
        bool CreateIndex();

        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        bool Add(TEntity entity, string name);

        /// <summary>
        /// 异步添加一条数据
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity entity, string name);

        /// <summary>  
        /// 将数据插入进数据库  
        /// </summary>  
        /// <param name="entity">需要插入数据库的具体实体</param>  
        /// <param name="name">指定插入的集合</param>  
        bool Insert(TEntity entity, string name);

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        bool AddList(List<TEntity> entities, string name);

        /// <summary>
        /// 异步批量插入
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        Task<bool> AddListAsync(List<TEntity> entities, string name);

        /// <summary>
        /// 异步批量插入海量数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        Task<bool> InsertManyAsync(List<TEntity> entities, string name);

        /// <summary>  
        /// 批量插入数据  
        /// </summary>  
        /// <param name="list">需要插入数据的列表</param>  
        /// <param name="name">指定要插入的集合</param>  
        bool InsertBatch(List<TEntity> list, string name);

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public UpdateResult Update(TEntity entity, string name, string id);

        /// <summary>
        /// 异步修改一条数据
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<UpdateResult> UpdateAsync(TEntity entity, string name, string id);

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
        Task<UpdateResult> UpdateAsync(TEntity entity, string name,Expression<Func<TEntity, bool>> filter);

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
        Task<UpdateResult> UpdateOneAsync(TEntity entity, string name, Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// 异步保存一条数据（存在则更新，不存在则插入，原子操作，无并发问题）
        /// 这是 MongoDB 官方推荐的方式，单条请求完成「判断+更新/插入」，彻底避免多线程/高并发场景下的竞态问题
        /// result.UpsertedId != null 表示插入成功，result.ModifiedCount > 0 表示更新成功
        /// </summary>
        /// <param name="entity">要保存的实体数据</param>
        /// <param name="name">MongoDB集合名称</param>
        /// <param name="filter">匹配条件（建议使用唯一条件，如_id或业务主键）</param>
        /// <returns>操作结果（可判断是新增还是更新）</returns>
        Task<UpdateResult> SaveAsync(TEntity entity, string name, Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// 替换单个文档
        /// </summary>
        /// <param name="entity">添加的实体</param>
        /// <param name="name">数据集</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        Task<ReplaceOneResult> ReplaceOneAsync(TEntity entity, string name,Expression<Func<TEntity, bool>> filter);

        /// <summary>  
        /// 更新数据  
        /// </summary>  
        /// <param name="query">更新数据的查询</param>  
        /// <param name="update">需要更新的文档</param>  
        /// <param name="name">指定更新集合的名称</param>  
        /// <param name="id">ObjectId的键</param>
        bool Update(FilterDefinition<TEntity> query, UpdateDefinition<TEntity> update, string name, string id);

        /// <summary>
        /// 批量修改数据
        /// </summary>
        /// <param name="dic">要修改的字段</param>
        /// <param name="name">数据集</param>
        /// <param name="filter">修改条件</param>
        /// <returns></returns>
        UpdateResult UpdateManay(Dictionary<string, string> dic, string name, FilterDefinition<TEntity> filter);

        /// <summary>
        /// 批量修改数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="name">数据集</param>
        /// <returns></returns>
        Task<int> UpdateManayAsync(List<TEntity> entities, string name);

        /// <summary>
        /// 异步批量修改数据
        /// </summary>
        /// <param name="dic">要修改的字段</param>
        /// <param name="name">数据集</param>
        /// <param name="filter">修改条件</param>
        /// <returns></returns>
        Task<UpdateResult> UpdateManayAsync(Dictionary<string, string> dic, string name, FilterDefinition<TEntity> filter);

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="id">objectId</param>
        /// <returns></returns>
        DeleteResult Delete(string name, string id);

        /// <summary>  
        /// 移除指定的数据  
        /// </summary>  
        /// <param name="query">移除的数据条件</param>  
        /// <param name="name">指定的集合名词</param>  
        bool Remove(FilterDefinition<TEntity> query, string name);

        /// <summary>
        /// 异步删除一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="id">objectId</param>
        /// <returns></returns>
        Task<DeleteResult> DeleteAsync(string name, string id);

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">删除的条件</param>
        /// <returns></returns>
        DeleteResult DeleteMany(string name, FilterDefinition<TEntity> filter);

        /// <summary>
        /// 根据条件删除数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="where">删除的条件</param>
        /// <returns></returns>
        long Delete(string name, Expression<Func<TEntity, bool>> where);

        /// <summary>
        /// 异步删除多条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">删除的条件</param>
        /// <returns></returns>
        Task<DeleteResult> DeleteManyAsync(string name, FilterDefinition<TEntity> filter);

        /// <summary>
        /// 异步删除多条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="where">删除的条件</param>
        /// <returns></returns>
        Task<long> DeleteAsync(string name, Expression<Func<TEntity, bool>> where);

        /// <summary>
        /// 根据id查询一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="id">objectid</param>
        /// <param name="field">要查询的字段，不写时查询全部</param>
        /// <returns></returns>
        TEntity Get(string name, string id, string[]? field = null);

        /// <summary>
        /// 根据条件获取一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="whereExpression">条件</param>
        /// <returns></returns>
        TEntity Get(string name, Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(string name, Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 异步根据id查询一条数据
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="id">objectid</param>
        /// <param name="field">要查询的字段，不写时查询全部</param>
        /// <returns></returns>
        Task<TEntity> GetAsync(string name, string id, string[]? field = null);

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        List<TEntity> GetList(string name, FilterDefinition<TEntity> filter, string[]? field = null, SortDefinition<TEntity>? sort = null);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetListAsync(string name, Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 根据条件获取条数
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="whereExpression">条件</param>
        /// <returns></returns>
        Task<long> CountAsync(string name, Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 异步查询集合
        /// </summary>
        /// <param name="name">数据集</param>
        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        Task<List<TEntity>> GetListAsync(string name, FilterDefinition<TEntity> filter, string[]? field = null, SortDefinition<TEntity>? sort = null);

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
        List<TEntity> GetList(string name, FilterDefinition<TEntity> filter, int pageIndex, int pageSize, out long count, string[]? field = null, SortDefinition<TEntity>? sort = null);

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
        Task<List<TEntity>> GetListAsync(string name, FilterDefinition<TEntity> filter, int pageIndex, int pageSize, string[]? field = null, SortDefinition<TEntity>? sort = null);

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
        Task<(List<TEntity>, int, int)> GetListAsync(string name, Expression<Func<TEntity, bool>> where,
            int pageIndex, int pageSize, string[]? field = null, SortDefinition<TEntity>? sort=null);
    }
}
