using Microsoft.AspNetCore.Mvc;
using Shipeng.Application.Contracts;
using Shipeng.Application.Contracts.Dtos;

namespace Shipeng.Hosting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefreshController : ControllerBase
    {
        private readonly IRefreshAppService _refreshAppService;
        public RefreshController(IRefreshAppService refreshAppService)
        {
            _refreshAppService = refreshAppService;
        }
        [HttpPost("/api/shipeng/refresh/configure")]
        public async Task<ShipengResult> ReloadConfigureAsync(RefreshConfigureDto refreshConfigure)
        {
            return await _refreshAppService.RefreshConfigureAsync(refreshConfigure);
        }
    }
}
