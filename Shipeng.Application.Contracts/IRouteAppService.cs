using Shipeng.Application.Contracts.Dtos.ReverseProxy;
using Volo.Abp.Application.Services;

namespace Shipeng.Application.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRouteAppService : IApplicationService
    {
        /// <summary>
        /// 获取全部路由数据
        /// </summary>
        /// <returns></returns>
        Task<ShipengResult<List<RouteMainDto>>> GetListAsync();
        /// <summary>
        /// 获取路由列表
        /// </summary>
        /// <param name="kw">关键字</param>
        /// <param name="page">当前页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns></returns>
        Task<ShipengPageResult<List<RoutePageDto>>> GetListAsync(string kw = "", int page = 1, int pageSize = 10);
        /// <summary>
        /// 根据路由ID获取路由信息
        /// </summary>
        /// <param name="routeId">路由ID</param>
        /// <returns></returns>
        Task<ShipengResult<RouteDto>> GetAsync(Guid routeId);
        /// <summary>
        /// 创建路由
        /// </summary>
        /// <param name="createRouteDto"></param>
        /// <returns></returns>
        Task<ShipengResult> CreateAsync(CreateRouteDto createRouteDto);
        /// <summary>
        /// 删除路由
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns></returns>
        Task<ShipengResult> DeleteAsync(Guid routeId);
        /// <summary>
        /// 更新路由信息
        /// </summary>
        /// <param name="updateRouteDto"></param>
        /// <returns></returns>
        Task<ShipengResult> UpdateAsync(UpdateRouteDto updateRouteDto);
        /// <summary>
        /// 更新路由状态
        /// </summary>
        /// <param name="routeId">路由ID</param>
        /// <param name="useState">路由状态</param>
        /// <returns></returns>
        Task<ShipengResult> UpdateStateAsync(Guid routeId, bool useState);
    }
}
