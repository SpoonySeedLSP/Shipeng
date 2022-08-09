using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Shipeng.Domain.ReverseProxy
{
    public class InDatabaseStoreConfig : IProxyConfig
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public InDatabaseStoreConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }
        public IReadOnlyList<RouteConfig> Routes { get; }

        public IReadOnlyList<ClusterConfig> Clusters { get; }

        public IChangeToken ChangeToken { get; internal set; }
        internal void SignalChange()
        {
            _cts.Cancel();
        }
    }
}
