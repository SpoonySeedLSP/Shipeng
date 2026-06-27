using Shipeng;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar.IOC;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar
{
    /// <summary>
    /// �Ƿ��� SqlSugar �ִ�
    /// </summary>
    public partial class SqlSugarRepository : ISqlSugarRepository
    {
        /// <summary>
        /// �����ṩ��
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="serviceProvider">�����ṩ��</param>
        public SqlSugarRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// �л��ִ�
        /// </summary>
        /// <typeparam name="TEntity">ʵ������</typeparam>
        /// <returns>�ִ�</returns>
        public virtual ISqlSugarRepository<TEntity> Change<TEntity>()
            where TEntity : class, new()
        {
            return _serviceProvider.GetService<ISqlSugarRepository<TEntity>>();
        }
    }

    /// <summary>
    /// SqlSugar �ִ�ʵ����
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial class SqlSugarRepository<TEntity> : ISqlSugarRepository<TEntity>
    where TEntity : class, new()
    {
        /// <summary>
        /// �Ƿ��� SqlSugar �ִ�
        /// </summary>
        private readonly ISqlSugarRepository _sqlSugarRepository;

        /// <summary>
        /// ��ʼ�� SqlSugar �ͻ���
        /// </summary>
        private readonly SqlSugarScope _db;

        /// <summary>
        /// ��ʼ�� ITenant �ͻ���
        /// </summary>
        private readonly ITenant _tenant;

        /// <summary>
        /// �⻧ID�������ݿ�����ID
        /// </summary>
        private readonly string tenantId;

        /// <summary>
        /// ���ݿ�����
        /// </summary>
        private readonly string tenantDbName;

        /// <summary>
        /// ���ݿ�����
        /// </summary>
        private readonly DbType dbType;

        /// <summary>
        /// ���ݿ�����
        /// </summary>
        private readonly IocDbType iocDbType;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="sqlSugarRepository"></param>
        /// <param name="db"></param>
        public SqlSugarRepository(ISqlSugarRepository sqlSugarRepository, ISqlSugarClient db)
        {
            _sqlSugarRepository = sqlSugarRepository;

            DynamicContext = Context = _db = (SqlSugarScope)db;
            Ado = _db.Ado;

            tenantId = App.Configuration["ConnectionStrings:ConfigId"];
            tenantDbName = App.Configuration["ConnectionStrings:DBName"];
            dbType = (DbType)Enum.Parse(typeof(DbType), App.Configuration["ConnectionStrings:DBType"]);
            iocDbType = (IocDbType)Enum.Parse(typeof(IocDbType), App.Configuration["ConnectionStrings:DBType"]);

            _tenant = _db;
            var httpContext = App.HttpContext;
            if (httpContext != null && httpContext.User.FindFirst("TenantId")?.Value != null)
            {
                tenantId = httpContext.User.FindFirst("TenantId")?.Value;
                tenantDbName = httpContext.User.FindFirst("TenantDbName")?.Value;
                _tenant.AddConnection(new ConnectionConfig()
                {
                    DbType = dbType,
                    ConfigId = tenantId,//���ÿ��Ψһ��ʶ
                    IsAutoCloseConnection = true,
                    ConnectionString = string.Format($"{App.Configuration["ConnectionStrings:DefaultConnection"]}", tenantDbName)
                });
                SugarIocServices.AddSqlSugar(new IocConfig()
                {
                    ConnectionString = string.Format($"{App.Configuration["ConnectionStrings:DefaultConnection"]}", tenantDbName),
                    DbType = iocDbType,
                    ConfigId = tenantId,
                    IsAutoCloseConnection = true//�Զ��ͷ�
                });
                //DbScoped.SugarScope.ChangeDatabase(tenantId);
            }
            else
            {
                tenantId = App.Configuration["ConnectionStrings:ConfigId"];
            }
            _tenant.ChangeDatabase(tenantId);

            //��ʽ���� ������ӡSQL 
            _db.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql + "\r\n" + _db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            };
        }

        /// <summary>
        /// ʵ�弯��
        /// </summary>
        public virtual ISugarQueryable<TEntity> Entities => _db.Queryable<TEntity>();

        /// <summary>
        /// ���ݿ�������
        /// </summary>
        public virtual SqlSugarScope Context { get; }

        /// <summary>
        /// ��̬���ݿ�������
        /// </summary>
        public virtual dynamic DynamicContext { get; }

        /// <summary>
        /// ԭ�� Ado ����
        /// </summary>
        public virtual IAdo Ado { get; }

        #region sql����
        /// <summary>
        /// SqlSugarͨ�õ�ִ�д洢���̷�������out��ʽ���ز���)
        /// </summary>
        /// <param name="sprocName">�洢������</param>
        /// <param name="intParams">�������</param>
        /// <param name="outParams">�������</param>
        public async Task<Dictionary<string, object>> ExecuteProcedureAsync(string sprocName,
            Dictionary<string, object> intParams, Dictionary<string, object> outParams)
        {
            List<SugarParameter> pList = new List<SugarParameter>();
            if (intParams != null)
            {
                pList = intParams.Select(
                    obj => new SugarParameter($@"@{obj.Key}", obj.Value))
                    .ToList();


            }
            if (outParams != null)
                pList.AddRange(
                    outParams.Select(
                        obj => new SugarParameter($@"@{obj.Key}", obj.Value)
                        {
                            Direction = ParameterDirection.Output
                        }));
            await _db.Ado.UseStoredProcedure().ExecuteCommandAsync(sprocName, pList);
            foreach (var p in pList.Where(r => r.Direction == ParameterDirection.Output))
            {
                var pName = p.ParameterName.Substring(1);
                if (outParams != null && outParams.ContainsKey(pName))
                {
                    outParams[pName] = p.Value;
                }
            }
            return outParams;
        }

        /// <summary>
        /// ִ�и���������
        /// </summary>
        /// <param name="sql">�����ַ���</param>
        /// <param name="parameters">ҪӦ���������ַ����Ĳ���</param>
        /// <returns>ִ������������ݿⷵ�صĽ��</returns>
        public async Task<int> ExecuteAsync(string sql, params object[] parameters)
        {
            return await _db.Ado.ExecuteCommandAsync(sql, parameters);
        }

        /// <summary>
        /// SqlServer��Go�Ľű�����
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="parameters">����</param>
        /// <returns></returns>
        public int ExecuteCommandWithGo(string sql, params SugarParameter[] parameters)
        {
            //go����Ƕ���һ�о�֧��
            return _db.Ado.ExecuteCommandWithGo(sql, parameters);
        }

        /// <summary>
        /// ����sql��ȡDataTable
        /// </summary>
        /// <param name="sql">��ѯsql���</param>
        /// <param name="parameters">����</param>
        /// <returns></returns>
        public async Task<DataTable> GetDataTableAsync(string sql, params SugarParameter[] parameters)
        {
            return await _db.Ado.GetDataTableAsync(sql, parameters);
        }

        /// <summary>
        /// ԭ��SQL��ȡ����
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="parameters">����</param>
        /// <returns></returns>
        public async Task<List<TEntity>> SqlQueryAsync(string sql, params object[] parameters)
        {
            return await _db.Ado.SqlQueryAsync<TEntity>(sql, parameters);
        }

        /// <summary>
        /// ԭ��SQL��ѯ2�������
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="parameters">����</param>
        /// <returns></returns>
        public async Task<Tuple<List<T1>, List<T2>>> SqlQueryAsync<T1, T2>(string sql, params object[] parameters)
        {
            return await _db.Ado.SqlQueryAsync<T1, T2>(sql, parameters);
        }

        /// <summary>
        /// ʹ��ԭ��sql��ҳ��ѯ
        /// �ڲ�ѯ������DTO�и��ֶμ�����ConditionsAttribute
        /// û��ConditionsAttribute����ʱ��������ֶ���������ͽ��в�ѯ��intʱ��=��stringʱ��like��datetimeʱ��������
        /// ������ʱ ����������һ������������������
        /// NotSelectΪ������ѯ����ʹDTO����ֵҲ�����ѯ
        /// Enable�Ƿ����ã���������õĻ���ͬû�����Ե��߼����ѷ�����
        /// symbolAttribute���ж��ַ��������������������в�ѯ��Ŀǰ��=��>,<,>=,<=,like����Χ�ȣ������뿴SymbolAttributeö��
        /// IsSplitΪ�Ƿ��зָ����ֻ���������Ϊ��Χʱ��Ч
        /// SplitString�Ƿָ�����ַ���ֻ����IsSplitΪtrueʱ��Ч
        /// ���⣬�˷�����֧���Զ���д��ѯ������������param�ж��岻��DTO�еĲ�ѯ������д��sql�ַ����м���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="sql"></param>
        /// <param name="queryParameter"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> SqlQueryAndParameterAsync<T, D>(string sql,
            QueryParameter<D> queryParameter, List<SqlParameter> param = null) where T : class, new() where D : class, new()
        {
            StringBuilder strsql = new StringBuilder();
            strsql.Append(@"select * from (" + sql + ") t where 1=1");
            if (param == null)
            {
                param = new List<SqlParameter>();
            }
            var datadto = queryParameter.data;
            if (datadto != null)
            {
                PropertyInfo[] propertys = datadto.GetType().GetProperties();
                foreach (var item in propertys)
                {
                    var attribute = item.GetCustomAttribute<ConditionsAttribute>();
                    var type = item.PropertyType.FullName;
                    var value = item?.GetValue(datadto);
                    var valueS = Convert.ToString(item?.GetValue(datadto));
                    if (string.IsNullOrWhiteSpace(valueS))
                    {
                        continue;
                    }
                    if (attribute == null)
                    {
                        if (type.Contains("Int"))
                        {
                            if (Convert.ToInt32(value) <= 0)
                            {
                                continue;
                            }
                            param.Add(new SqlParameter("@" + item.Name, value));
                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                        }
                        else if (type.Contains("String"))
                        {
                            param.Add(new SqlParameter("@" + item.Name, "%" + value + "%"));
                            strsql.Append(" and t." + item.Name + " like @" + item.Name);
                        }
                        else if (type.Contains("Datetime"))
                        {
                            var date = ((DateTime)value).ToString("yyyy/MM/dd");
                            param.Add(new SqlParameter("@" + item.Name, date));
                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                        }
                        else
                        {
                            param.Add(new SqlParameter("@" + item.Name, value));
                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                        }
                    }
                    else
                    {
                        if (!attribute.NotSelect)
                        {
                            var conditionsType = attribute.ConditionsTypes;
                            var symbolAttribute = attribute.SymbolAttributes;

                            switch (symbolAttribute)
                            {
                                case SymbolAttribute.EQUAL:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " = @" + item.Name);
                                    break;
                                case SymbolAttribute.CONTAILS:
                                    param.Add(new SqlParameter("@" + item.Name, "%" + value + "%"));
                                    strsql.Append(" and t." + item.Name + " like @" + item.Name);
                                    break;
                                case SymbolAttribute.STARTSWITH:
                                    param.Add(new SqlParameter("@" + item.Name, "%" + value));
                                    strsql.Append(" and t." + item.Name + " like @" + item.Name);
                                    break;
                                case SymbolAttribute.ENDSWITH:
                                    param.Add(new SqlParameter("@" + item.Name, value + "%"));
                                    strsql.Append(" and t." + item.Name + " like @" + item.Name);
                                    break;
                                case SymbolAttribute.GREATER:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " > @" + item.Name);
                                    break;
                                case SymbolAttribute.LESS:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " < @" + item.Name);
                                    break;
                                case SymbolAttribute.GREATEREQUAL:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " >= @" + item.Name);
                                    break;
                                case SymbolAttribute.LESSEQUAL:
                                    param.Add(new SqlParameter("@" + item.Name, value));
                                    strsql.Append(" and t." + item.Name + " <= @" + item.Name);
                                    break;
                                case SymbolAttribute.INTERVAL:
                                    if (attribute.IsSplit)
                                    {
                                        string[] timestring = valueS.Split(attribute.SplitString);
                                        if (timestring != null)
                                        {
                                            if (timestring.Length == 1)
                                            {
                                                param.Add(new SqlParameter("@" + item.Name + "start", timestring[0]));
                                                strsql.Append(" and (t." + item.Name + " >= @" + item.Name + "start )");
                                            }
                                            else if (timestring.Length == 2)
                                            {
                                                if (timestring[0] != "" && timestring[1] != "")
                                                {
                                                    param.Add(new SqlParameter("@" + item.Name + "start", timestring[0]));
                                                    param.Add(new SqlParameter("@" + item.Name + "end", timestring[1]));
                                                    strsql.Append(" and (t." + item.Name + " >= @" + item.Name + "start and t." + item.Name + " <= @" + item.Name + "end)");
                                                }
                                                else if (timestring[0] != "" && timestring[1] == "")
                                                {
                                                    param.Add(new SqlParameter("@" + item.Name + "start", timestring[0]));
                                                    strsql.Append(" and (t." + item.Name + " >= @" + item.Name + "start )");
                                                }
                                                else if (timestring[0] == "" && timestring[1] != "")
                                                {
                                                    param.Add(new SqlParameter("@" + item.Name + "start", timestring[1]));
                                                    strsql.Append(" and ( t." + item.Name + " <= @" + item.Name + "end)");
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    switch (conditionsType)
                                    {
                                        case ConditionsType.INT:
                                            param.Add(new SqlParameter("@" + item.Name, value));
                                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                                            break;
                                        case ConditionsType.STRING:
                                            param.Add(new SqlParameter("@" + item.Name, "%" + value + "%"));
                                            strsql.Append(" and t." + item.Name + " like @" + item.Name);
                                            break;
                                        case ConditionsType.DATETIME:
                                            var date = ((DateTime)value).ToString("yyyy/MM/dd");
                                            param.Add(new SqlParameter("@" + item.Name, date));
                                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                                            break;
                                        default:
                                            param.Add(new SqlParameter("@" + item.Name, value));
                                            strsql.Append(" and t." + item.Name + " = @" + item.Name);
                                            break;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            queryParameter.Count = (await _db.Ado.SqlQueryAsync<T>(strsql.ToString(), param.ToArray())).Count();
            if (queryParameter.sidx == "")
            {
                queryParameter.sidx = "F_Id";
                queryParameter.sord = "desc";
            }
            strsql.Append($@" order by {queryParameter.sidx} {queryParameter.sord} offset {(queryParameter.pageNum - 1) * queryParameter.pageSize} rows fetch next {queryParameter.pageSize} rows only");
            return await _db.Ado.SqlQueryAsync<T>(strsql.ToString(), param.ToArray());
        }

        /// <summary>
        /// �洢���̷�ҳ��ѯ
        /// </summary>
        /// <param name="intParams">�������</param>
        /// <returns>��ҳ���ͼ���</returns>
        public async Task<SqlSugarPagedList<TEntity>> ProcedureToPageListAsync(Dictionary<string, object> intParams)
        {
            //�������
            var outParams = new Dictionary<string, object>
                {
                    { "TotalCount", 0 },//�Ƿ����ܼ�¼
                    { "TotalPageCount", 0 }//������ҳ��
                };
            var parameters = new List<SugarParameter>();
            parameters = intParams.Select(
               obj => new SugarParameter($@"@{obj.Key}", obj.Value))
               .ToList();
            parameters.AddRange(
               outParams.Select(
                   obj => new SugarParameter($@"@{obj.Key}", obj.Value)
                   {
                       Direction = ParameterDirection.Output
                   }));
            var result = await _db.Ado.UseStoredProcedure().SqlQueryAsync<TEntity>("P_viewPage", parameters);
            var pageList = new SqlSugarPagedList<TEntity>();
            pageList.list = result;
            foreach (var p in parameters.Where(r => r.Direction == ParameterDirection.Output))
            {
                var pName = p.ParameterName.Substring(1);
                if (outParams != null && outParams.ContainsKey(pName))
                {
                    outParams[pName] = p.Value;
                }
            }
            pageList.pagination = new PagedModel()
            {
                PageIndex = (int)intParams["PageIndex"],
                PageSize = (int)intParams["PageSize"],
                Total = (int)outParams["TotalCount"],
                PageCount = (int)outParams["TotalPageCount"]
            };
            return pageList;
        }
        #endregion

        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public int Count(Expression<Func<TEntity, bool>> whereExpression)
        {
            return Entities.Count(whereExpression);
        }

        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression)
        {
            return Entities.CountAsync(whereExpression);
        }

        /// <summary>
        /// ����Ƿ����
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public bool Any(Expression<Func<TEntity, bool>> whereExpression)
        {
            return Entities.Any(whereExpression);
        }

        /// <summary>
        /// ����Ƿ����
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await Entities.AnyAsync(whereExpression);
        }

        /// <summary>
        /// ͨ��������ȡʵ��
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public TEntity Single(dynamic Id)
        {
            return Entities.InSingle(Id);
        }

        /// <summary>
        /// ��ȡһ��ʵ��
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public TEntity Single(Expression<Func<TEntity, bool>> whereExpression)
        {
            return Entities.Single(whereExpression);
        }

        /// <summary>
        /// ��ȡһ��ʵ��
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> whereExpression)
        {
            return Entities.SingleAsync(whereExpression);
        }

        /// <summary>
        /// ��ȡһ��ʵ��
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> whereExpression)
        {
            return Entities.First(whereExpression);
        }

        /// <summary>
        /// ��ȡһ��ʵ��
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await Entities.FirstAsync(whereExpression);
        }

        /// <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <returns></returns>
        public List<TEntity> ToList()
        {
            return Entities.ToList();
        }

        /// <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public List<TEntity> ToList(Expression<Func<TEntity, bool>> whereExpression)
        {
            return Entities.Where(whereExpression).ToList();
        }

        /// <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="orderByType"></param>
        /// <returns></returns>
        public List<TEntity> ToList(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            return Entities.OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(whereExpression).ToList();
        }

        /// <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <returns></returns>
        public Task<List<TEntity>> ToListAsync()
        {
            return Entities.ToListAsync();
        }

        /// <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> whereExpression)
        {
            return Entities.Where(whereExpression).ToListAsync();
        }

        /// <summary>
        /// ���ݱ��ʽ������In��ѯ����������ȡ����(�����б���ǻ���)
        /// </summary>
        /// <param name="where">���ʽ����</param>
        /// <param name="conditions">�����б��ÿ��key����һ����ѯ������ÿ��key��Ӧһ���б�ֵ����IN��ѯ</param>
        /// <param name="batchSize">ÿ���δ���ĸ���</param>
        /// <returns></returns>
        public List<TEntity> ToListAsync(Expression<Func<TEntity, bool>> where, Dictionary<string, List<string>> conditions, int batchSize = 1000)
        {
            Stopwatch watch = Stopwatch.StartNew(); // ���ڼ�ʱ
            watch.Start();
            List<string> batchValues;
            var query = _db.Queryable<TEntity>();
            var list = new List<TEntity>();
            var result = new List<TEntity>();
            Parallel.ForEach(conditions.AsParallel(), condition => {
                if (string.IsNullOrWhiteSpace(condition.Key) || !condition.Value.Any()) return;

                for (int i = 0; i < condition.Value.Count; i += batchSize)
                {
                    batchValues = condition.Value.Skip(i).Take(batchSize).ToList();
                    if (batchValues.Any())
                    {
                        list = _db.Queryable<TEntity>().In(condition.Key, batchValues).Where(where).ToList();
                        if (list.Any()) result.AddRange(list);
                    }
                }
            });
            watch.Stop();
            return result;
        }

        /// <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="orderByType"></param>
        /// <returns></returns>
        public Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            return Entities.OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(whereExpression).ToListAsync();
        }

        /// <summary>
        /// ����һ����¼
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual int Insert(TEntity entity)
        {
            return _db.Insertable(entity).ExecuteCommand();
        }

        /// <summary>
        /// ����������¼
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int Insert(params TEntity[] entities)
        {
            return _db.Insertable(entities).ExecuteCommand();
        }

        /// <summary>
        /// ����������¼
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int Insert(IEnumerable<TEntity> entities)
        {
            return _db.Insertable(entities.ToArray()).ExecuteCommand();
        }

        /// <summary>
        /// ����һ����¼��������Id
        /// </summary>
        /// <param name="insertObj"></param>
        /// <returns></returns>
        public int InsertReturnIdentity(TEntity insertObj)
        {
            return _db.Insertable(insertObj).ExecuteReturnIdentity();
        }

        /// <summary>
        /// ����һ����¼
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual Task<int> InsertAsync(TEntity entity)
        {
            return _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// ����������¼
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual Task<int> InsertAsync(params TEntity[] entities)
        {
            return _db.Insertable(entities).ExecuteCommandAsync();
        }

        /// <summary>
        /// ����������¼
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual Task<int> InsertAsync(IEnumerable<TEntity> entities)
        {
            if (entities != null && entities.Any())
            {
                return _db.Insertable(entities.ToArray()).ExecuteCommandAsync();
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// �첽�������뺣������
        /// </summary>
        /// <param name="entities">���ݼ�</param>
        /// <param name="batchSize">������������Ĵ�С</param>
        /// <returns></returns>
        public async Task<int> BatchInsertAsync(List<TEntity> entities, int batchSize = 10000)
        {
            if (!entities.Any()) return 0;

            _db.Ado.BeginTran(); // ��������
            try
            {
                Stopwatch watch = Stopwatch.StartNew(); // ���ڼ�ʱ
                var count = entities.Count;
                var result = 0;
                watch.Start();
                for (int i = 0; i < count; i += batchSize)
                {
                    // ��ȡ��ǰ���ε�����
                    var items = entities.GetRange(i, Math.Min(batchSize, count - i));

                    // ִ����������
                    result = result + await _db.Insertable(items).ExecuteCommandAsync();
                }

                _db.Ado.CommitTran(); // �ύ����
                watch.Stop();
                return result;
            }
            catch
            {
                _db.Ado.RollbackTran(); // ����ع�
                throw;
            }
        }

        /// <summary>
        /// ����һ����¼��������Id
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<long> InsertReturnIdentityAsync(TEntity entity)
        {
            return await _db.Insertable(entity).ExecuteReturnBigIdentityAsync();
        }

        /// <summary>
        /// ����һ����¼
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual int Update(TEntity entity)
        {
            return _db.Updateable(entity).ExecuteCommand();
        }

        /// <summary>
        /// ���¶�����¼
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int Update(params TEntity[] entities)
        {
            return _db.Updateable(entities).ExecuteCommand();
        }

        /// <summary>
        /// ���¶�����¼
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int Update(IEnumerable<TEntity> entities)
        {
            return _db.Updateable(entities.ToArray()).ExecuteCommand();
        }

        /// <summary>
        /// ����һ����¼
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual Task<int> UpdateAsync(TEntity entity)
        {
            return _db.Updateable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// �첽����һ����¼��
        ///
        /// �������
        /// 1. ���ʵ��Ϊ�գ�ֱ�ӷ��� 0��
        /// 2. ���ʵ������Ϊ�գ�ִ��������
        /// 3. ���ʵ��������Ϊ�գ��������ݿ��д��ڸ�������ִ�и��¡�
        /// 4. ���ʵ��������Ϊ�գ������ݿ��в����ڸ�������ִ��������
        ///
        /// �ɹ��жϣ�
        /// 1. �����ɹ���Insertable ����Ӱ���������� 0��
        /// 2. ���³ɹ���Updateable ����Ӱ���������� 0��
        /// 3. �������������ݿ�ԭ������ȫһ��ʱ���������ݿ�/�������ܷ��� 0��
        ///    ��ֻҪ��������¼ȷʵ���ڣ�ҲӦ��Ϊ����ɹ���
        ///
        /// ʹ�ó�����
        /// 1. ��ҵ���Ѿ������� Id������ȷ�����ݿ����Ƿ���ڸü�¼��
        /// 2. ͬ������ʱ��ϣ������������£�����������������
        /// 3. ������÷�ÿ�ζ��ֶ� AnyAsync �жϡ�
        ///
        /// ע�⣺
        /// 1. �÷���Ĭ�ϰ�ʵ�������ж��Ƿ���ڡ�
        /// 2. ���ҵ����Ҫ������Ψһ�ֶ��жϣ�������ʹ�ø÷�����Ӧ����дҵ���жϡ�
        /// 3. ����ֵ���� 0 ��ʾ����ɹ������� 0 һ���ʾʵ��Ϊ�ա�
        /// </summary>
        /// <param name="entity">ʵ�����</param>
        /// <returns>Ӱ������������ 1 ��ʾ����ɹ�</returns>
        public virtual async Task<int> SaveAsync(TEntity entity)
        {
            if (entity == null)
                return 0;

            // ��ȡ SqlSugar ʵ��Ԫ���ݣ�������ȡ�����ֶΡ�
            var entityInfo = _db.EntityMaintenance.GetEntityInfo<TEntity>();

            // ��ȡ�����ֶΡ�
            var primaryKey = entityInfo.Columns.FirstOrDefault(x => x.IsPrimarykey);

            if (primaryKey == null)
                throw new Exception($"{typeof(TEntity).Name}ʵ��δ�����������޷�ִ�б��������");

            // ��ȡ�����ֶ�ֵ��
            var idValue = primaryKey.PropertyInfo.GetValue(entity);

            // ����Ϊ��ʱ��˵���������ݣ�ֱ��ִ��������
            if (idValue == null || string.IsNullOrWhiteSpace(idValue.ToString()))
            {
                var insertRows = await _db.Insertable(entity).ExecuteCommandAsync();
                return insertRows > 0 ? insertRows : 0;
            }

            // ������Ϊ��ʱ�����ж����ݿ����Ƿ���ڸ�������
            // ����ʹ�� InSingleAsync����������ѯ�����ܱ�ƴ���ʽ��ֱ�ӡ�
            var oldEntity = await _db.Queryable<TEntity>().InSingleAsync(idValue);

            // ���ݿ��в����ڸ�������ִ��������
            if (oldEntity == null)
            {
                var insertRows = await _db.Insertable(entity).ExecuteCommandAsync();
                return insertRows > 0 ? insertRows : 0;
            }

            // ���ݿ��д��ڸ�������ִ�и��¡�
            var updateRows = await _db.Updateable(entity).ExecuteCommandAsync();

            // updateRows > 0�����ݿ�ȷʵ�������޸ģ�����ɹ���
            if (updateRows > 0)
                return updateRows;

            // updateRows == 0�������Ǳ������ݺ����ݿ�ԭ������ȫһ�¡�
            // ֻҪ�ɼ�¼���ڣ���˵����α����ҵ��Ƕ��ǳɹ��ġ�
            return 1;
        }

        /// <summary>
        /// ���¶�����¼
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual Task<int> UpdateAsync(params TEntity[] entities)
        {
            return _db.Updateable(entities).ExecuteCommandAsync();
        }

        /// <summary>
        /// ���¶�����¼
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual Task<int> UpdateAsync(IEnumerable<TEntity> entities)
        {
            return _db.Updateable(entities.ToArray()).ExecuteCommandAsync();
        }

        /// <summary>
        /// ɾ��һ����¼
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual int Delete(TEntity entity)
        {
            return _db.Deleteable(entity).ExecuteCommand();
        }

        /// <summary>
        /// ɾ��һ����¼
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual int Delete(object key)
        {
            return _db.Deleteable<TEntity>().In(key).ExecuteCommand();
        }

        /// <summary>
        /// ɾ��������¼
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual int Delete(params object[] keys)
        {
            return _db.Deleteable<TEntity>().In(keys).ExecuteCommand();
        }

        /// <summary>
        /// �Զ�������ɾ����¼
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public int Delete(Expression<Func<TEntity, bool>> whereExpression)
        {
            return _db.Deleteable<TEntity>().Where(whereExpression).ExecuteCommand();
        }

        /// <summary>
        /// ɾ��һ����¼
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual Task<int> DeleteAsync(TEntity entity)
        {
            return _db.Deleteable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// ɾ��һ����¼
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual Task<int> DeleteAsync(object key)
        {
            return _db.Deleteable<TEntity>().In(key).ExecuteCommandAsync();
        }

        /// <summary>
        /// ɾ��������¼
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual Task<int> DeleteAsync(params object[] keys)
        {
            return _db.Deleteable<TEntity>().In(keys).ExecuteCommandAsync();
        }

        /// <summary>
        /// �Զ�������ɾ����¼
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await _db.Deleteable<TEntity>().Where(whereExpression).ExecuteCommandAsync();
        }

        /// <summary>
        /// ���ݱ��ʽ��ѯ������¼
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual ISugarQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return AsQueryable(predicate);
        }

        /// <summary>
        /// ���ݱ��ʽ��ѯ������¼
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual ISugarQueryable<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> predicate)
        {
            return AsQueryable().WhereIF(condition, predicate);
        }

        /// <summary>
        /// ������ѯ������
        /// </summary>
        /// <returns></returns>
        public virtual ISugarQueryable<TEntity> AsQueryable()
        {
            return Entities;
        }

        /// <summary>
        /// ������ѯ������
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual ISugarQueryable<TEntity> AsQueryable(Expression<Func<TEntity, bool>> predicate)
        {
            return Entities.Where(predicate);
        }

        /// <summary>
        /// ֱ�ӷ������ݿ���
        /// </summary>
        /// <returns></returns>
        public virtual List<TEntity> AsEnumerable()
        {
            return AsQueryable().ToList();
        }

        /// <summary>
        /// ֱ�ӷ������ݿ���
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual List<TEntity> AsEnumerable(Expression<Func<TEntity, bool>> predicate)
        {
            return AsQueryable(predicate).ToList();
        }

        /// <summary>
        /// ֱ�ӷ������ݿ���
        /// </summary>
        /// <returns></returns>
        public virtual Task<List<TEntity>> AsAsyncEnumerable()
        {
            return AsQueryable().ToListAsync();
        }

        /// <summary>
        /// ֱ�ӷ������ݿ���
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual Task<List<TEntity>> AsAsyncEnumerable(Expression<Func<TEntity, bool>> predicate)
        {
            return AsQueryable(predicate).ToListAsync();
        }

        /// <summary>
        /// �л��ִ�
        /// </summary>
        /// <typeparam name="TChangeEntity">ʵ������</typeparam>
        /// <returns>�ִ�</returns>
        public virtual ISqlSugarRepository<TChangeEntity> Change<TChangeEntity>()
            where TChangeEntity : class, new()
        {
            return _sqlSugarRepository.Change<TChangeEntity>();
        }
    }
}