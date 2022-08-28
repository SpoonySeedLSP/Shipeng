using Shipeng.Application.Contracts.Dtos.Whitelist;
using Volo.Abp.Application.Services;

namespace Shipeng.Application.Contracts
{
    /// <summary>
    /// 白名单
    /// </summary>
    public interface IWhitelistAppService : IApplicationService
    {
        /// <summary>
        /// 获取白名单详情信息
        /// </summary>
        /// <param name="id">白名单ID</param>
        /// <returns></returns>
        Task<ShipengResult<WhitelistDto>> GetAsync(Guid id);
        /// <summary>
        /// 更新启用状态
        /// </summary>
        /// <param name="id">白名单ID</param>
        /// <param name="useState">状态</param>
        /// <returns></returns>
        Task<ShipengResult> UpdateUseStateAsync(Guid id, bool useState);
        /// <summary>
        /// 更新白名单数据
        /// </summary>
        /// <param name="updateWhiteList"></param>
        /// <returns></returns>
        Task<ShipengResult> UpdateAsync(UpdateWhitelistDto updateWhiteList);
        /// <summary>
        /// 获取白名单列表
        /// </summary>
        /// <param name="kw"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<ShipengPageResult<List<WhitelistDto>>> GetListAsync(string kw = "", int page = 1, int pageSize = 10);
        /// <summary>
        /// 删除白名单
        /// </summary>
        /// <param name="id">白名单ID</param>
        /// <returns></returns>
        Task<ShipengResult> DeleteAsync(Guid id);
        /// <summary>
        /// 创建白名单
        /// </summary>
        /// <param name="createWhiteList"></param>
        /// <returns></returns>
        Task<ShipengResult> CreateAsync(CreateWhitelistDto createWhiteList);
    }
}
