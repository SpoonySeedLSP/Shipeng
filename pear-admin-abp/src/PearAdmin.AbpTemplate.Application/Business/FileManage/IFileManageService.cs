using Abp.Application.Services;
using Abp.Application.Services.Dto;
using PearAdmin.AbpTemplate.Business.FileManage.Dto;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Business.FileManage
{
    /// <summary>
    /// 文件管理/档案管理服务
    /// </summary>
    public interface IFileManageService: IApplicationService
    {
        /// <summary>
        /// 分页获取文件管理/档案管理数据
        /// </summary>
        /// <param name="input">分页查询文件管理/档案管理模型</param>
        /// <returns>文件管理/档案管理数据传输模型</returns>
        Task<PagedResultDto<FileManageListDto>> GetPagedFileManageListAsync(QueryPagedFileManageInput input);

        /// <summary>
        /// 根据id获取文件/档案信息
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>文件/档案信息传输模型</returns>
        Task<FileManageInfoDto> GetFileManageInfoAsync(long id);

        /// <summary>
        /// 删除附件文件
        /// </summary>
        /// <param name="id">唯一标识id</param>
        /// <returns></returns>
        Task DeleteFileAsync(long id);

        /// <summary>
        /// 获取文件文件信息
        /// </summary>
        /// <param name="id">唯一标识id</param>
        /// <returns>Item1.附件文件信息 Item2.文件绝对路径</returns>
        Task<(FileListsDto, string)> GetFileInfoAsync(long id);

    }
}
