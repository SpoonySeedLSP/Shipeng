using Microsoft.Extensions.Primitives;

namespace Shipeng.Domain.ReverseProxy
{
    public class InDatabaseReloadToken : IChangeToken
    {
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        public void OnReload() => tokenSource.Cancel();
        public bool ActiveChangeCallbacks { get; } = true;

        public bool HasChanged { get { return tokenSource.IsCancellationRequested; } }

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return tokenSource.Token.Register(callback, state);
        }
    }
}
