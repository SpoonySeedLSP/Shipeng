using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Shipeng.Domain.Entities;

namespace Shipeng.Domain.ReverseProxy
{
    internal class RouteManager : DomainService, IRouteManager
    {
        private readonly IRepository<Route> _routeRepository;
        public RouteManager(IRepository<Route> routeRepository)
        {
            _routeRepository = routeRepository;
        }

        /// <summary>
        /// 创建路由 
        /// </summary>
        /// <param name="routeName">路由名称</param>
        /// <param name="routeMatchPath">路由路径</param>
        /// <param name="useState">服务状态</param>
        /// <param name="description">路由描述</param>
        /// <returns>服务配置表</returns>
        public async Task<Route> CreateAsync(string routeName, string routeMatchPath, bool useState, string description)
        {
            var route = await _routeRepository.FirstOrDefaultAsync(x => x.RouteName == routeName || x.RouteMatchPath == routeMatchPath);
            if (route != null)
            {
                throw new Exception("路由名称或者路由路径已经存在");
            }
            var id = GuidGenerator.Create();
            return new Route(id)
            {
                Created = DateTime.Now,
                RouteMatchPath = routeMatchPath,
                UseState = useState,
                RouteName = routeName,
                Updated = DateTime.Now,
                Description = string.IsNullOrEmpty(description) ? "" : description,
                RouteId = id.ToString(),
            };
        }

        /// <summary>
        /// 创建路由交换
        /// </summary>
        /// <param name="routeId">路由ID</param>
        /// <param name="transformsName">交换项名称</param>
        /// <param name="transformsValue">交换项值</param>
        /// <returns>服务交换配置表</returns>
        public Task<RouteTransform> CreateRouteTransformAsync(Guid routeId, string transformsName, string transformsValue)
        {
            return Task.Run(() =>
            {
                return new RouteTransform(GuidGenerator.Create())
                {
                    RouteId = routeId,
                    TransformsName = transformsName,
                    TransformsValue = transformsValue
                };
            });
        }
    }
}
