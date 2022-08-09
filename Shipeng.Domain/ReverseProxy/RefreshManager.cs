using Volo.Abp.Domain.Services;

namespace Shipeng.Domain.ReverseProxy
{
    internal class RefreshManager : DomainService, IRefreshManager
    {
        private readonly IReverseProxyDatabaseStore _reverseProxyMemoryStore;
        public RefreshManager(IReverseProxyDatabaseStore reverseProxyMemoryStore)
        {
            _reverseProxyMemoryStore = reverseProxyMemoryStore;
        }
        public Task ReloadConfigAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                _reverseProxyMemoryStore.Reload();
            });
        }
    }
}
