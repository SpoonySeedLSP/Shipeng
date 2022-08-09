using Volo.Abp.Domain.Repositories;
using Mapster;
using Consul;
using Volo.Abp.Uow;
using Serilog;
using Shipeng.Domain.Entities;
using Shipeng.Domain.Shared.Enums;
using Shipeng.Domain.Shared.Options;
using Volo.Abp.Domain.Services;
using Shipeng.Domain.ReverseProxy.Models;

namespace Shipeng.Domain.ReverseProxy
{
    internal class YarpManager : DomainService, IYarpManager
    {
        private readonly IRepository<Route> _routeRepository;
        private readonly IRepository<RouteTransform> _routeTransformRepository;
        private readonly IRepository<Cluster> _clusterRepository;
        private readonly IRepository<ClusterDestination> _clusterDestinationRepository;
        private readonly IRepository<ClusterHealthCheck> _clusterHealthCheckRepository;
        private readonly IRepository<ServiceGovernanceConfigure> _serviceGovernanceRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IHttpClientFactory _httpClientFactory;
        //
        private ServiceGovernanceModel? _serviceGovernanceModel;
        private ConsulClient? _consulClient;
        public YarpManager(IRepository<Route> routeRepository, IRepository<RouteTransform> routeTransformRepository
            , IRepository<Cluster> clusterRepository, IRepository<ClusterDestination> clusterDestinationRepository
            , IUnitOfWorkManager unitOfWorkManager, IRepository<ClusterHealthCheck> clusterHealthCheckRepository, IRepository<ServiceGovernanceConfigure> serviceGovernanceRepository
            , IHttpClientFactory httpClientFactory)
        {
            _routeRepository = routeRepository;
            _routeTransformRepository = routeTransformRepository;
            _clusterRepository = clusterRepository;
            _clusterDestinationRepository = clusterDestinationRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _clusterHealthCheckRepository = clusterHealthCheckRepository;
            _serviceGovernanceRepository = serviceGovernanceRepository;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<YarpOption> GetConfigureAsync()
        {
            var result = new YarpOption()
            {
                Routes = new List<RouteOption>()
            };
            using var unitOfWork = _unitOfWorkManager.Begin();
            //获取所有数据
            var routes = await _routeRepository.GetListAsync(x => x.UseState);
            var routeTransforms = await _routeTransformRepository.GetListAsync();
            var clusters = await _clusterRepository.GetListAsync();
            var clusterDestinations = await _clusterDestinationRepository.GetListAsync();
            var clusterHealthChecks = await _clusterHealthCheckRepository.GetListAsync();
            //
            if (_serviceGovernanceModel == null)
            {
                _serviceGovernanceModel = (await _serviceGovernanceRepository.GetQueryableAsync())
                    .ProjectToType<ServiceGovernanceModel>()
                    .FirstOrDefault();
            }
            //处理数据集
            RouteOption? routeOption;
            Cluster? cluster;
            ClusterHealthCheck? clusterHealthCheck;
            foreach (var route in routes)
            {
                routeOption = new RouteOption()
                {
                    RouteId = route.RouteId,
                    RouteMatchPath = route.RouteMatchPath,
                    RouteName = route.RouteName,
                    RouteTransforms = routeTransforms.Where(x => x.RouteId == route.Id).Select(x => new RouteTransformOption()
                    {
                        TransformsName = x.TransformsName,
                        TransformsValue = x.TransformsValue
                    })
                    .ToList()
                };
                //集群相关数据
                cluster = clusters.Where(x => x.RouteId == route.Id).FirstOrDefault();
                routeOption.Cluster = new ClusterOption()
                {
                    ClusterName = cluster?.ClusterName??"",
                    LoadBalancingPolicy = cluster?.LoadBalancingPolicy??""
                };
                if (cluster!=null && cluster.ServiceGovernanceType == ServiceGovernanceType.Consul)
                {
                    var consulDestinations = await GetConsulServiceAsync(cluster.ServiceGovernanceName);
                    if (consulDestinations == null)
                    {
                        continue;
                    }
                    routeOption.Cluster.ClusterDestinations = consulDestinations;
                }
                else if (cluster!=null && cluster.ServiceGovernanceType == ServiceGovernanceType.Nacos)
                {
                    var consulDestinations = await GetNacosServiceAsync(cluster.ServiceGovernanceName);
                    if (consulDestinations == null)
                    {
                        continue;
                    }
                    routeOption.Cluster.ClusterDestinations = consulDestinations;
                }
                else
                {
                    routeOption.Cluster.ClusterDestinations = clusterDestinations.Where(x => x.ClusterId == cluster?.Id)
                        .Select(x => new ClusterDestinationOption()
                        {
                            DestinationAddress = x.DestinationAddress,
                            DestinationName = x.DestinationName
                        })
                        .ToList();
                }
                if (cluster != null && clusterHealthChecks!=null && clusterHealthChecks.Count>0)
                {
                    clusterHealthCheck = clusterHealthChecks.Where(x => x.ClusterId == cluster.Id)?.ToList().FirstOrDefault();
                    if (clusterHealthCheck!=null)
                        routeOption.Cluster.ClusterHealthCheck = new ClusterHealthCheckOption()
                        {
                            Enabled = clusterHealthCheck.Enabled,
                            Interval = clusterHealthCheck.Interval,
                            Path = clusterHealthCheck.Path,
                            Policy = clusterHealthCheck.Policy,
                            Timeout = clusterHealthCheck.Timeout
                        };
                }                         
                //
                result.Routes.Add(routeOption);
            }
            return result;
        }
        private async Task<List<ClusterDestinationOption>?> GetConsulServiceAsync(string serviceGovernanceName)
        {

            if (_serviceGovernanceModel == null)
            {
                return null;
            }
            if (_consulClient == null)
            {
                var consulClientConfiguration = new ConsulClientConfiguration();
                consulClientConfiguration.Datacenter = _serviceGovernanceModel.ConsulDatacenter;
                consulClientConfiguration.Address = new Uri(_serviceGovernanceModel.ConsulServer);
                if (!string.IsNullOrEmpty(_serviceGovernanceModel.ConsulToken) && _serviceGovernanceModel.ConsulToken != "")
                {
                    consulClientConfiguration.Token = _serviceGovernanceModel.ConsulToken;
                }
                _consulClient = new ConsulClient(consulClientConfiguration);
            }
            try
            {
                var queryResult = await _consulClient.Catalog.Service(serviceGovernanceName);
                if (queryResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Log.Error(new NotImplementedException(), $"未发现名称为{serviceGovernanceName}的服务");
                    return null;
                }
                var servcies = queryResult.Response;
                if (servcies == null || !servcies.Any())
                {
                    Log.Error(new NotImplementedException(), $"名称为{serviceGovernanceName}的服务未包含任何节点");
                    return null;
                }
                var destinations = servcies.Select(x => new ClusterDestinationOption()
                {
                    DestinationAddress = $"http://{x.ServiceAddress}:{x.ServicePort}",
                    DestinationName = Guid.NewGuid().ToString().Replace("-", "")
                })
                .ToList();
                return destinations;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Consul连接异常:{ex.Message}");
                return null;
            }
        }
        private async Task<List<ClusterDestinationOption>?> GetNacosServiceAsync(string serviceGovernanceName)
        {

            if (_serviceGovernanceModel == null)
            {
                return null;
            }
            try
            {
                var requestData = $"serviceName={serviceGovernanceName}";
                if (!string.IsNullOrEmpty(_serviceGovernanceModel.NacosNamespaceId) && _serviceGovernanceModel.NacosNamespaceId != "")
                {
                    requestData += $"&namespaceId={_serviceGovernanceModel.NacosNamespaceId}";
                }
                if (!string.IsNullOrEmpty(_serviceGovernanceModel.NacosGroupName) && _serviceGovernanceModel.NacosGroupName != "")
                {
                    requestData += $"&groupName={_serviceGovernanceModel.NacosGroupName}";
                }
                //
                var httpClient = _httpClientFactory.CreateClient();
                var httpResponse = await httpClient.GetAsync($"{_serviceGovernanceModel.NacosServer}/nacos/v1/ns/instance/list?{requestData}");
                if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Log.Error(new NotImplementedException(), $"从Nacos获取服务信息失败,服务名:{serviceGovernanceName}|命名空间{_serviceGovernanceModel?.NacosNamespaceId}|群组名称:{_serviceGovernanceModel.NacosGroupName}");
                    return null;
                }
                var httpResult = Newtonsoft.Json.JsonConvert.DeserializeObject<NacosServiceModel>(await httpResponse.Content.ReadAsStringAsync());
                var destinations = httpResult?.Hosts.Select(x => new ClusterDestinationOption()
                {
                    DestinationAddress = $"http://{x.IP}:{x.Port}",
                    DestinationName = Guid.NewGuid().ToString().Replace("-", "")
                })
                .ToList();
                return destinations;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Nacos服务发现异常:{ex.Message}");
                return null;
            }
        }

    }
}
