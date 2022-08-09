namespace Shipeng.Domain.Whitelist
{
    public interface IWhiteListManager
    {
        Task<List<Entities.Whitelist>> GetListAsync();
        /// <summary>
        /// 创建白名单
        /// </summary>
        /// <param name="whiteList"></param>
        /// <returns></returns>
        Task<Entities.Whitelist> CreateAsync<T>(T whiteList);
    }
}
