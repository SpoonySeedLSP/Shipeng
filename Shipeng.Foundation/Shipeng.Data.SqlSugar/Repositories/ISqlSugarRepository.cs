using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SqlSugar
{
    /// <summary>
    /// 非泛型 SqlSugar 仓储
    /// </summary>
    public partial interface ISqlSugarRepository
    {
        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        ISqlSugarRepository<TEntity> Change<TEntity>()
            where TEntity : class, new();
    }

    /// <summary>
    /// SqlSugar 仓储接口定义
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial interface ISqlSugarRepository<TEntity>
        where TEntity : class, new()
    {
        /// <summary>
        /// 实体集合
        /// </summary>
        ISugarQueryable<TEntity> Entities { get; }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        SqlSugarScope Context { get; }

        /// <summary>
        /// 动态数据库上下文
        /// </summary>
        dynamic DynamicContext { get; }

        /// <summary>
        /// 原生 Ado 对象
        /// </summary>
        IAdo Ado { get; }

        #region sql操作
        /// <summary>
        /// SqlSugar通用的执行存储过程方法（以out方式返回参数)
        /// </summary>
        /// <param name="sprocName">存储过程名</param>
        /// <param name="intParams">输入参数</param>
        /// <param name="outParams">输出参数</param>
        Task<Dictionary<string, object>> ExecuteProcedureAsync(string sprocName, Dictionary<string, object> intParams, Dictionary<string, object> outParams);

        /// <summary>
        /// 执行给定的命令
        /// </summary>
        /// <param name="sql">命令字符串</param>
        /// <param name="parameters">要应用于命令字符串的参数</param>
        /// <returns>执行命令后由数据库返回的结果</returns>
        Task<int> ExecuteAsync(string sql, params object[] parameters);

        /// <summary>
        /// SqlServer带Go的脚本处理
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        int ExecuteCommandWithGo(string sql, params SugarParameter[] parameters);

        /// <summary>
        /// 根据sql获取DataTable
        /// </summary>
        /// <param name="sql">查询sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        Task<DataTable> GetDataTableAsync(string sql, params SugarParameter[] parameters);

        /// <summary>
        /// 原生SQL获取数据
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        Task<List<TEntity>> SqlQueryAsync(string sql, params object[] parameters);

        /// <summary>
        /// 原生SQL查询2个结果集
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        Task<Tuple<List<T1>, List<T2>>> SqlQueryAsync<T1, T2>(string sql, params object[] parameters);

        /// <summary>
        /// 使用原生sql分页查询
        /// 在查询条件的DTO中给字段加特性ConditionsAttribute
        /// 没有ConditionsAttribute特性时，会根据字段自身的类型进行查询，int时按=，string时按like，datetime时搜索当天
        /// 有特性时 跟根据特性一定的特性来进行搜索
        /// NotSelect为永不查询，即使DTO中有值也不会查询
        /// Enable是否启用，如果不启用的话，同没有特性的逻辑（已废弃）
        /// symbolAttribute是判断字符运算符，根据运算符进行查询，目前有=，>,<,>=,<=,like，范围等，详情请看SymbolAttribute枚举
        /// IsSplit为是否有分隔符，只有在运算符为范围时生效
        /// SplitString是分隔符的字符，只有在IsSplit为true时生效
        /// 另外，此方法还支持自定义写查询条件，可以在param中定义不在DTO中的查询条件，写在sql字符串中即可
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="sql"></param>
        /// <param name="queryParameter"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> SqlQueryAndParameterAsync<T, D>(string sql,
            QueryParameter<D> queryParameter, List<SqlParameter> param = null) where T : class, new() where D : class, new();

        /// <summary>
        /// 存储过程分页查询
        /// </summary>
        /// <param name="intParams">输入参数</param>
        /// <returns>分页泛型集合</returns>
        Task<SqlSugarPagedList<TEntity>> ProcedureToPageListAsync(Dictionary<string, object> intParams);
        #endregion

        /// <summary>
        /// 获取总数
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        int Count(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 获取总数
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 检查是否存在
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        bool Any(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 检查是否存在
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 通过主键获取实体
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        TEntity Single(dynamic Id);

        /// <summary>
        /// 获取一个实体
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        TEntity Single(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 获取一个实体
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 获取一个实体
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 获取一个实体
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        List<TEntity> ToList();

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        List<TEntity> ToList(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="orderByType"></param>
        /// <returns></returns>
        List<TEntity> ToList(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        Task<List<TEntity>> ToListAsync();

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 根据表达式条件和In查询参数批量获取数据(参数列表都是或者)
        /// </summary>
        /// <param name="where">表达式条件</param>
        /// <param name="conditions">参数列表，每个key代表一个查询条件，每个key对应一个列表值用于IN查询</param>
        /// <param name="batchSize">每批次处理的个数</param>
        /// <returns></returns>
        List<TEntity> ToListAsync(Expression<Func<TEntity, bool>> where, Dictionary<string, List<string>> conditions, int batchSize = 1000);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="orderByType"></param>
        /// <returns></returns>
        Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc);

        /// <summary>
        /// 新增一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Insert(TEntity entity);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Insert(params TEntity[] entities);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Insert(IEnumerable<TEntity> entities);

        /// <summary>
        /// 新增一条记录返回自增Id
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int InsertReturnIdentity(TEntity entity);

        /// <summary>
        /// 新增一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> InsertAsync(TEntity entity);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> InsertAsync(params TEntity[] entities);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> InsertAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// 异步批量插入海量数据
        /// </summary>
        /// <param name="entities">数据集</param>
        /// <param name="batchSize">设置批量插入的大小</param>
        /// <returns></returns>
        Task<int> BatchInsertAsync(List<TEntity> entities, int batchSize = 10000);

        /// <summary>
        /// 新增一条记录返回自增Id
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<long> InsertReturnIdentityAsync(TEntity entity);

        /// <summary>
        /// 更新一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Update(TEntity entity);

        /// <summary>
        /// 更新多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Update(params TEntity[] entities);

        /// <summary>
        /// 更新多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Update(IEnumerable<TEntity> entities);

        /// <summary>
        /// 更新一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(TEntity entity);

        /// <summary>
        /// 异步保存一条记录。
        ///
        /// 保存规则：
        /// 1. 如果实体为空，直接返回 0。
        /// 2. 如果实体主键为空，执行新增。
        /// 3. 如果实体主键不为空，并且数据库中存在该主键，执行更新。
        /// 4. 如果实体主键不为空，但数据库中不存在该主键，执行新增。
        ///
        /// 成功判断：
        /// 1. 新增成功：Insertable 返回影响行数大于 0。
        /// 2. 更新成功：Updateable 返回影响行数大于 0。
        /// 3. 更新内容与数据库原内容完全一致时，部分数据库/驱动可能返回 0，
        ///    但只要该主键记录确实存在，也应视为保存成功。
        ///
        /// 使用场景：
        /// 1. 主业务已经生成了 Id，但不确定数据库中是否存在该记录。
        /// 2. 同步数据时，希望“存在则更新，不存在则新增”。
        /// 3. 避免调用方每次都手动 AnyAsync 判断。
        ///
        /// 注意：
        /// 1. 该方法默认按实体主键判断是否存在。
        /// 2. 如果业务需要按其它唯一字段判断，不建议使用该方法，应单独写业务判断。
        /// 3. 返回值大于 0 表示保存成功；返回 0 一般表示实体为空。
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>影响行数，返回 1 表示保存成功</returns>
        Task<int> SaveAsync(TEntity entity);

        /// <summary>
        /// 更新多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(params TEntity[] entities);

        /// <summary>
        /// 更新多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Delete(TEntity entity);

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        int Delete(object key);

        /// <summary>
        /// 删除多条记录
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        int Delete(params object[] keys);

        /// <summary>
        /// 自定义条件删除记录
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        int Delete(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(TEntity entity);

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(object key);

        /// <summary>
        /// 删除多条记录
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(params object[] keys);

        /// <summary>
        /// 自定义条件删除记录
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// 根据表达式查询多条记录
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        ISugarQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 根据表达式查询多条记录
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        ISugarQueryable<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <returns></returns>
        ISugarQueryable<TEntity> AsQueryable();

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        ISugarQueryable<TEntity> AsQueryable(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <returns></returns>
        List<TEntity> AsEnumerable();

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        List<TEntity> AsEnumerable(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <returns></returns>
        Task<List<TEntity>> AsAsyncEnumerable();

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<List<TEntity>> AsAsyncEnumerable(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeEntity"></typeparam>
        /// <returns></returns>
        ISqlSugarRepository<TChangeEntity> Change<TChangeEntity>() where TChangeEntity : class, new();
    }
}