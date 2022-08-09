using Shipeng.Domain.ReverseProxy;
using Shipeng.Application.Contracts;
using Shipeng.Domain;
using Shipeng.Application.Contracts.Dtos;

namespace Shipeng.Application
{
    public class RefreshAppService : BaseApplicationService, IRefreshAppService
    {
        private readonly IRefreshManager _refreshManager;
        private readonly IConfigureManager _configureManager;
        public RefreshAppService( IConfigureManager configureManager, IRefreshManager refreshManager)
        {
            _configureManager = configureManager;
            _refreshManager = refreshManager;
        }
        public async Task<ShipengResult> RefreshConfigureAsync(RefreshConfigureDto refreshConfigure)
        {
            //加载基础配置
            if (refreshConfigure.Authentication != null)
            {
                _configureManager.ReloadAuthentication(refreshConfigure.Authentication);
            }
            if (refreshConfigure.Whitelists!=null)
            {
                _configureManager.ReloadWhitelist(refreshConfigure.Whitelists);
            }
            if (refreshConfigure.Middlewares != null)
            {
                _configureManager.ReloadMiddleware(refreshConfigure.Middlewares);
            }
            //加载路由等数据
            if (refreshConfigure.Yarp != null)
            {
                _configureManager.ReloadYayp(refreshConfigure.Yarp);
                await _refreshManager.ReloadConfigAsync();
            }
            return Ok();
        }
    }
}
