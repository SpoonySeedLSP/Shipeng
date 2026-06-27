using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shipeng.DatabaseIndex
{
    /// <summary>
    /// 数据库索引后台自动检查服务。
    ///
    /// 启动流程：
    /// 1. 应用启动。
    /// 2. 判断 DatabaseIndex:EnableAutoEnsure 是否开启。
    /// 3. 如果未开启，直接退出，不做任何数据库操作。
    /// 4. 如果开启，等待 StartupDelaySeconds 秒。
    /// 5. 创建 DI Scope。
    /// 6. 调用 IDatabaseIndexEnsureService.EnsureAsync()。
    ///
    /// 为什么使用 HostedService：
    /// 1. 不阻塞 Startup.ConfigureServices。
    /// 2. 不影响 WebAPI 路由初始化。
    /// 3. 可以延迟执行，降低启动瞬间压力。
    /// </summary>
    public class DatabaseIndexHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DatabaseIndexOptions _options;
        private readonly ILogger<DatabaseIndexHostedService>? _logger;

        /// <summary>
        /// 构造函数。
        /// </summary>
        public DatabaseIndexHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<DatabaseIndexOptions> options,
            ILogger<DatabaseIndexHostedService>? logger = null)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory), "IServiceScopeFactory 不能为空。");
            _options = options?.Value ?? new DatabaseIndexOptions();
            _logger = logger;
        }

        /// <summary>
        /// 后台执行入口。
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.EnableAutoEnsure)
            {
                _logger?.LogInformation("数据库索引自动检查未启用。需要启用时请配置 DatabaseIndex:EnableAutoEnsure=true。");
                return;
            }

            try
            {
                var delaySeconds = _options.StartupDelaySeconds < 0 ? 0 : _options.StartupDelaySeconds;

                if (delaySeconds > 0)
                {
                    _logger?.LogInformation("数据库索引自动检查将在 {Seconds} 秒后开始。", delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested)
                    return;

                using var scope = _scopeFactory.CreateScope();
                var ensureService = scope.ServiceProvider.GetRequiredService<IDatabaseIndexEnsureService>();

                var result = await ensureService.EnsureAsync(stoppingToken);

                if (result.Success)
                {
                    _logger?.LogInformation(
                        "数据库索引自动检查完成。SQL Server 新建 {SqlCount} 个，MongoDB 新建 {MongoCount} 个。",
                        result.SqlServerCreatedCount,
                        result.MongoCreatedCount);
                }
                else
                {
                    _logger?.LogWarning(
                        "数据库索引自动检查完成，但存在失败项。失败数量：{ErrorCount}。",
                        result.Errors.Count);

                    foreach (var error in result.Errors)
                        _logger?.LogWarning(error);

                    if (_options.ThrowWhenFailed)
                        throw new Exception("数据库索引自动检查失败：" + string.Join("；", result.Errors));
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation("应用正在停止，数据库索引自动检查已取消。");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "数据库索引自动检查发生异常。");

                if (_options.ThrowWhenFailed)
                    throw;
            }
        }
    }
}