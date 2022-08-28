using Shipeng.Application.Contracts.Dtos.Node;
using Volo.Abp.Application.Services;

namespace Shipeng.Application.Contracts
{
    public interface INodeAppService : IApplicationService
    {
        /// <summary>
        /// 获取所有节点数据
        /// </summary>
        /// <returns></returns>
        Task<ShipengResult<List<NodeDto>>> GetAllAsync();
        /// <summary>
        /// 获取节点详情信息
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns></returns>
        Task<ShipengResult<NodeDto>> GetAsync(Guid id);
        /// <summary>
        /// 更新节点数据
        /// </summary>
        /// <param name="updateNode"></param>
        /// <returns></returns>
        Task<ShipengResult> UpdateAsync(UpdateNodeDto updateNode);
        /// <summary>
        /// 获取节点列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<ShipengPageResult<List<NodeDto>>> GetListAsync(int page = 1, int pageSize = 10);
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns></returns>
        Task<ShipengResult> DeleteAsync(Guid id);
        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="createNode"></param>
        /// <returns></returns>
        Task<ShipengResult> CreateAsync(CreateNodeDto createNode);
    }
}
