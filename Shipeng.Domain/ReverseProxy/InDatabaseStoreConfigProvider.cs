using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Shipeng.Domain.ReverseProxy
{
    public class InDatabaseStoreConfigProvider : IProxyConfigProvider, IDisposable
    {
        private readonly object _lockObject = new object();
        private readonly IReverseProxyDatabaseStore _strore;
        private InDatabaseStoreConfig _config;
        private CancellationTokenSource _changeToken;
        private bool _disposed;
        private IDisposable _subscription;

        private ILogger<InDatabaseStoreConfigProvider> _logger;
        public InDatabaseStoreConfigProvider(IReverseProxyDatabaseStore strore, ILogger<InDatabaseStoreConfigProvider> logger)
        {
            _strore = strore;
            _logger = logger;
        }
        public IProxyConfig GetConfig()
        {
            // First time load
            if (_config == null)
            {
                _subscription = ChangeToken.OnChange(_strore.GetReloadToken, UpdateConfig);
                UpdateConfig();
            }
            return _config;
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _subscription?.Dispose();
                _changeToken?.Dispose();
                _disposed = true;
            }
        }
        private void UpdateConfig()
        {
            // 防止重复更新，尤其是在启动时
            lock (_lockObject)
            {
                InDatabaseStoreConfig newConfig;
                try
                {
                    newConfig = _strore.GetConfig();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    return;
                }

                var oldToken = _changeToken;
                _changeToken = new CancellationTokenSource();
                newConfig.ChangeToken = new CancellationChangeToken(_changeToken.Token);
                _config = newConfig;
                try
                {
                    oldToken?.Cancel(throwOnFirstException: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
