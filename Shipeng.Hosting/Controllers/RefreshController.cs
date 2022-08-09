using Shipeng.Application;
using Shipeng.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
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
        [HttpPost("/api/kite/refresh/configure")]
        public async Task<ShipengResult> ReloadConfigureAsync(RefreshConfigureDto refreshConfigure)
        {
            return await _refreshAppService.RefreshConfigureAsync(refreshConfigure);
        }
    }
}
