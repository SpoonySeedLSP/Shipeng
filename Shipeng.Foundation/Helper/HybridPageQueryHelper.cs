using Mapster;
using MongoDB.Driver;
using SqlSugar;
using System.Linq.Expressions;
using Shipeng.Data.MongoDB.Repositorie;

namespace Shipeng.Util
{
    /// <summary>
    /// MongoDB + SQL Server 混合分页查询帮助类。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 本类用于统一封装“优先查询 MongoDB，MongoDB 当前条件没有任何数据时再回查 SQL Server”
    /// 的分页查询模式。它适合 MongoDB 作为读库、缓存库或宽表库，SQL Server/SqlSugar 仓储作为
    /// 最终兜底数据源的业务系统。
    /// </para>
    /// <para>
    /// 注意：是否回查 SQL Server 不能根据当前页列表是否为空判断，而要根据 MongoDB 的总数判断。
    /// 例如 MongoDB 总数为 100，前端传入第 999 页时，当前页列表为空，但这说明页码越界，
    /// 不是 MongoDB 没有数据，因此不能回查 SQL Server。
    /// </para>
    /// <para>
    /// 性能建议：MongoDB 的过滤字段和排序字段应建立组合索引；SQL Server 的 where/order by
    /// 字段也应建立对应索引。两端排序规则应尽量一致，否则同一条件下的分页顺序可能不一致。
    /// </para>
    /// </remarks>
    public static class HybridPageQueryHelper
    {
        /// <summary>
        /// MongoDB 优先、SQL Server 兜底的分页查询，并将实体映射为 DTO。
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <typeparam name="TDto">返回 DTO 类型。</typeparam>
        /// <param name="mongoRepository">MongoDB 仓储。</param>
        /// <param name="sqlRepository">SqlSugar 仓储。</param>
        /// <param name="mongoCollectionName">MongoDB 集合名称。</param>
        /// <param name="where">查询条件，建议同时兼容 MongoDB 和 SqlSugar。</param>
        /// <param name="currentPage">当前页码，从 1 开始；小于等于 0 时自动修正为 1。</param>
        /// <param name="pageSize">每页数量；小于等于 0 时自动修正为 20。</param>
        /// <param name="mongoSort">MongoDB 排序规则。</param>
        /// <param name="sqlOrderBy">SQL 排序规则，由调用方根据业务字段指定。</param>
        /// <param name="afterMapAsync">DTO 映射后的业务补充回调，例如补充名称、状态文本、统计字段。</param>
        /// <returns>分页结果。</returns>
        public static async Task<SqlSugarPagedList<TDto>> QueryPagedAsync<TEntity, TDto>(
            IMongoDBRepository<TEntity> mongoRepository,
            ISqlSugarRepository<TEntity> sqlRepository,
            string mongoCollectionName,
            Expression<Func<TEntity, bool>> where,
            int currentPage,
            int pageSize,
            SortDefinition<TEntity> mongoSort = null,
            Func<ISugarQueryable<TEntity>, ISugarQueryable<TEntity>> sqlOrderBy = null,
            Func<List<TDto>, Task> afterMapAsync = null)
            where TEntity : class, new()
            where TDto : class, new()
        {
            ValidateArguments(mongoRepository, sqlRepository, mongoCollectionName, where);
            NormalizePage(ref currentPage, ref pageSize);

            SqlSugarPagedList<TDto> pageList = CreateEmptyPage<TDto>(currentPage, pageSize);

            (List<TEntity> mongoEntities, int mongoTotal, int mongoPageCount) =
                await QueryMongoAsync(mongoRepository, mongoCollectionName, where, currentPage, pageSize, mongoSort).ConfigureAwait(false);

            if (mongoTotal > 0)
            {
                List<TDto> dtoList = mongoEntities.Adapt<List<TDto>>();
                pageList.list = dtoList;
                pageList.pagination = CreatePagination(currentPage, pageSize, mongoTotal, mongoPageCount);

                if (afterMapAsync != null && dtoList.Count > 0)
                {
                    await afterMapAsync(dtoList).ConfigureAwait(false);
                }

                return pageList;
            }

            ISugarQueryable<TEntity> sqlQuery = sqlRepository.Entities.Where(where);
            if (sqlOrderBy != null)
            {
                sqlQuery = sqlOrderBy(sqlQuery);
            }

            SqlSugarPagedList<TEntity> sqlResult = await sqlQuery.ToPagedListAsync(currentPage, pageSize).ConfigureAwait(false);
            List<TDto> sqlDtoList = sqlResult.list.Adapt<List<TDto>>();

            pageList.list = sqlDtoList;
            pageList.pagination = sqlResult.pagination;

            if (afterMapAsync != null && sqlDtoList.Count > 0)
            {
                await afterMapAsync(sqlDtoList).ConfigureAwait(false);
            }

            return pageList;
        }

        /// <summary>
        /// MongoDB 优先、SQL Server 兜底的分页查询，不做 DTO 映射，直接返回实体分页结果。
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <param name="mongoRepository">MongoDB 仓储。</param>
        /// <param name="sqlRepository">SqlSugar 仓储。</param>
        /// <param name="mongoCollectionName">MongoDB 集合名称。</param>
        /// <param name="where">查询条件，建议同时兼容 MongoDB 和 SqlSugar。</param>
        /// <param name="currentPage">当前页码，从 1 开始；小于等于 0 时自动修正为 1。</param>
        /// <param name="pageSize">每页数量；小于等于 0 时自动修正为 20。</param>
        /// <param name="mongoSort">MongoDB 排序规则。</param>
        /// <param name="sqlOrderBy">SQL 排序规则，由调用方根据业务字段指定。</param>
        /// <param name="afterQueryAsync">查询完成后的业务补充回调。</param>
        /// <returns>分页结果。</returns>
        public static async Task<SqlSugarPagedList<TEntity>> QueryPagedAsync<TEntity>(
            IMongoDBRepository<TEntity> mongoRepository,
            ISqlSugarRepository<TEntity> sqlRepository,
            string mongoCollectionName,
            Expression<Func<TEntity, bool>> where,
            int currentPage,
            int pageSize,
            SortDefinition<TEntity> mongoSort = null,
            Func<ISugarQueryable<TEntity>, ISugarQueryable<TEntity>> sqlOrderBy = null,
            Func<List<TEntity>, Task> afterQueryAsync = null)
            where TEntity : class, new()
        {
            ValidateArguments(mongoRepository, sqlRepository, mongoCollectionName, where);
            NormalizePage(ref currentPage, ref pageSize);

            (List<TEntity> mongoEntities, int mongoTotal, int mongoPageCount) =
                await QueryMongoAsync(mongoRepository, mongoCollectionName, where, currentPage, pageSize, mongoSort).ConfigureAwait(false);

            if (mongoTotal > 0)
            {
                var pageList = CreateEmptyPage<TEntity>(currentPage, pageSize);
                pageList.list = mongoEntities;
                pageList.pagination = CreatePagination(currentPage, pageSize, mongoTotal, mongoPageCount);

                if (afterQueryAsync != null && mongoEntities.Count > 0)
                {
                    await afterQueryAsync(mongoEntities).ConfigureAwait(false);
                }

                return pageList;
            }

            ISugarQueryable<TEntity> sqlQuery = sqlRepository.Entities.Where(where);
            if (sqlOrderBy != null)
            {
                sqlQuery = sqlOrderBy(sqlQuery);
            }

            SqlSugarPagedList<TEntity> sqlResult = await sqlQuery.ToPagedListAsync(currentPage, pageSize).ConfigureAwait(false);
            if (afterQueryAsync != null && sqlResult.list.Any())
            {
                await afterQueryAsync(sqlResult.list.ToList()).ConfigureAwait(false);
            }

            return sqlResult;
        }

        private static async Task<(List<TEntity> Entities, int Total, int PageCount)> QueryMongoAsync<TEntity>(
            IMongoDBRepository<TEntity> mongoRepository,
            string mongoCollectionName,
            Expression<Func<TEntity, bool>> where,
            int currentPage,
            int pageSize,
            SortDefinition<TEntity> mongoSort)
            where TEntity : class, new()
        {
            (List<TEntity> entities, int total, int pageCount) = await mongoRepository
                .GetListAsync(mongoCollectionName, where, currentPage, pageSize, null, mongoSort)
                .ConfigureAwait(false);

            return (entities ?? new List<TEntity>(), total, pageCount);
        }

        private static void ValidateArguments<TEntity>(
            IMongoDBRepository<TEntity> mongoRepository,
            ISqlSugarRepository<TEntity> sqlRepository,
            string mongoCollectionName,
            Expression<Func<TEntity, bool>> where)
            where TEntity : class, new()
        {
            if (mongoRepository == null)
            {
                throw new ArgumentNullException(nameof(mongoRepository), "MongoDB 仓储不能为空。");
            }

            if (sqlRepository == null)
            {
                throw new ArgumentNullException(nameof(sqlRepository), "SqlSugar 仓储不能为空。");
            }

            if (string.IsNullOrWhiteSpace(mongoCollectionName))
            {
                throw new ArgumentNullException(nameof(mongoCollectionName), "MongoDB 集合名称不能为空。");
            }

            if (where == null)
            {
                throw new ArgumentNullException(nameof(where), "查询条件不能为空。");
            }
        }

        private static void NormalizePage(ref int currentPage, ref int pageSize)
        {
            currentPage = currentPage <= 0 ? 1 : currentPage;
            pageSize = pageSize <= 0 ? 20 : pageSize;
        }

        private static SqlSugarPagedList<T> CreateEmptyPage<T>(int currentPage, int pageSize)
            where T : class, new()
        {
            return new SqlSugarPagedList<T>
            {
                list = new List<T>(),
                pagination = CreatePagination(currentPage, pageSize, 0, 0)
            };
        }

        private static PagedModel CreatePagination(int currentPage, int pageSize, int total, int pageCount)
        {
            return new PagedModel
            {
                PageIndex = currentPage,
                PageSize = pageSize,
                Total = total,
                PageCount = pageCount
            };
        }
    }
}
