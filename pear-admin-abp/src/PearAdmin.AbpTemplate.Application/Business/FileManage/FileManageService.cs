using Abp;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using PearAdmin.AbpTemplate.Authorization.Users;
using PearAdmin.AbpTemplate.Business.FileManage.Dto;
using PearAdmin.AbpTemplate.Common.Enum;
using PearAdmin.AbpTemplate.Organizations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Business.FileManage
{
    /// <summary>
    /// 文件管理/档案管理服务的实现
    /// </summary>
    public class FileManageService : AbpTemplateApplicationServiceBase, IFileManageService
    {
        private readonly IUserAppService _userAppService;//用户应用服务接口
        private readonly IOrganizationUnitAppService _organizationUnitAppService;//组织机构应用服务
        private readonly IRepository<FileManageEntity, long> _fileManageRepository;//文件管理/档案管理信息仓储
        private readonly IRepository<FileArticleContentEntity, long> _fileArticleContentRepository;//文件文章信息仓储
        private readonly IRepository<FileListsEntity, long> _fileListsRepository;//文件明细信息仓储

        public FileManageService(IUserAppService userAppService,
            IOrganizationUnitAppService organizationUnitAppService,
            IRepository<FileManageEntity, long> fileManageRepository,
            IRepository<FileArticleContentEntity, long> fileArticleContentRepository,
            IRepository<FileListsEntity, long> fileListsRepository)
        {
            _userAppService = userAppService;
            _organizationUnitAppService = organizationUnitAppService;
            _fileManageRepository = fileManageRepository;
            _fileArticleContentRepository = fileArticleContentRepository;
            _fileListsRepository = fileListsRepository;
        }

        /// <summary>
        /// 分页获取文件管理/档案管理数据
        /// </summary>
        /// <param name="input">分页查询文件管理/档案管理模型</param>
        /// <returns>文件管理/档案管理数据传输模型</returns>
        public async Task<PagedResultDto<FileManageListDto>> GetPagedFileManageListAsync(QueryPagedFileManageInput input)
        {
            try
            {             
                var query =  _fileManageRepository.GetAll()
                    .WhereIf(1==1,r => r.HostId == 1 && r.TenantId == AbpSession.TenantId);
                if (query == null || query.Count() <= 0) return null;
                #region 数据权限判断
                if (input.OrganizationId.HasValue)
                {
                    #region 选组织查询
                    //文件公开类型(null.全部 0.完全公开 1.部门内部公开 2.仅自己可见)
                    if (input.FilePublicType == null || input.FilePublicType==FilePublicTypeEnum.FullyPublic)
                    {
                        //可以看到自己的所有数据、完全公开的数据、部门内部公开的数据
                        query = query.WhereIf(1==1,r=>r.UserId== (long)AbpSession.UserId || 
                        (r.FilePublicType== FilePublicTypeEnum.FullyPublic) ||
                        (r.OrganizationId== input.OrganizationId && r.FilePublicType== FilePublicTypeEnum.Public));
                    }
                    else if(input.FilePublicType== FilePublicTypeEnum.Public)
                    {
                        //可以看到自己的所有数据、本部门内部公开的数据
                        query = query.WhereIf(1==1,r => r.UserId == (long)AbpSession.UserId ||
                        (r.OrganizationId == input.OrganizationId && r.FilePublicType == FilePublicTypeEnum.Public));
                    }
                    else
                    {
                        //只能看到所选组织的并且是自己的并且是未公开的数据
                        query = query.WhereIf(1==1,r => r.UserId == (long)AbpSession.UserId &&
                        r.FilePublicType == FilePublicTypeEnum.Private);
                    }
                   #endregion
                }
                else
                {
                    #region 没有选组织查询
                    //获取当前登录用户所在组织及所有下级组织id
                    var orgs = await _organizationUnitAppService.GetOrganizationUnitListByUserIdAsync((long)AbpSession.UserId);
                    if (orgs == null || orgs.Count <= 0) return null;
                    //文件公开类型(null.全部 0.完全公开 1.部门内部公开 2.仅自己可见)
                    if (input.FilePublicType == null || input.FilePublicType == FilePublicTypeEnum.FullyPublic)
                    {
                        //可以到自己的所有数据、部门内部的所有公开数据、完全公开的数据
                        query = query.WhereIf(1==1,r => r.UserId == (long)AbpSession.UserId || 
                        (orgs == null || orgs.Count <= 0 ? 
                         r.FilePublicType == FilePublicTypeEnum.FullyPublic : 
                        (r.FilePublicType == FilePublicTypeEnum.FullyPublic || 
                        (orgs.Select(r=>r.Id).ToList().Contains(r.OrganizationId) && r.FilePublicType== FilePublicTypeEnum.Public))
                        ));
                    }
                    else if (input.FilePublicType == FilePublicTypeEnum.Public)
                    {
                        //可以看到自己的所有数据、所有部门内部公开的数据
                        query = query.WhereIf(1==1,r => r.UserId == (long)AbpSession.UserId || 
                        (orgs == null || orgs.Count <= 0 ? 1 == 2 :
                        orgs.Select(r => r.Id).ToList().Contains(r.OrganizationId) &&
                        r.FilePublicType == FilePublicTypeEnum.Public));
                    }
                    else
                    {
                        //只能看到组织内部的并且是自己的并且是未公开的数据
                        query = query.WhereIf(1==1,r => (r.UserId == (long)AbpSession.UserId && 
                        r.FilePublicType == FilePublicTypeEnum.Private) || 
                        (orgs == null || orgs.Count <= 0 ? 1 == 2 :
                        (r.UserId == (long)AbpSession.UserId &&
                        orgs.Select(r => r.Id).ToList().Contains(r.OrganizationId) &&
                        r.FilePublicType == FilePublicTypeEnum.Private))
                        );
                    }
                    #endregion
                }
                #endregion
                if (query == null || query.Count() <= 0) return null;
                query = query.WhereIf(input.ClassifyId.HasValue, r=>r.ClassifyId==input.ClassifyId)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.KeyWord),
                    r => r.Title.Contains(input.KeyWord) || r.Describe.Contains(input.KeyWord))
                    .WhereIf(input.StartDate != null,r => r.CreationTime >= input.StartDate)
                    .WhereIf(input.EndDate != null, r => r.CreationTime <= input.EndDate);
                if (query == null || query.Count() <= 0) return null;

                var totalCount = await query.CountAsync();
                var list = await query.OrderByDescending(r=>r.CreationTime).PageBy(input).ToListAsync();
                if (list == null || list.Count <= 0) return null;
                var result= ObjectMapper.Map<List<FileManageListDto>>(list);
                result = await DisposeFileManageListAsync(result);
                return new PagedResultDto<FileManageListDto>(totalCount, result);
            }
            catch (Exception ex)
            {
                throw new AbpException(message: $"分页获取文件管理/档案管理数据发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 处理文件管理/档案管理数据
        /// </summary>
        /// <param name="result">文件管理/档案管理数据</param>
        /// <returns></returns>
        private async Task<List<FileManageListDto>> DisposeFileManageListAsync(List<FileManageListDto> result)
        {
            try
            {
                var orgIds = result.Select(r => r.OrganizationId).Distinct().ToList();
                var orgs = await _organizationUnitAppService.GetOrganizationUnitListByIdsAsync(orgIds);
                var userIds = result.Select(r => r.UserId).Distinct().ToList();
                var users = await _userAppService.GetUserListByIdsAsync(userIds);
                foreach (var item in result)
                {
                    item.OrganizationName = orgs?.FirstOrDefault(r => r.Id == item.OrganizationId)?.DisplayName;
                    item.Name = users?.FirstOrDefault(r => r.Id == item.UserId)?.Name;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new AbpException(message: $"处理文件管理/档案管理数据发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 根据id获取文件/档案信息
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>文件/档案信息传输模型</returns>
        public async Task<FileManageInfoDto> GetFileManageInfoAsync(long id)
        {
            try
            {
                //获取文件管理/档案管理信息
                var entity = await _fileManageRepository.FirstOrDefaultAsync(id);
                //UserFriendlyException：继承自AbpException类, 实现了IHasLogSeverity接口
                //用户可以通过UserFriendlyException的实例来封装需要返回给客户端的异常
                if (entity==null) throw new UserFriendlyException(L("找不到文件管理/档案管理信息！"));
                var result = ObjectMapper.Map<FileManageInfoDto>(entity);
                //获取文件文章信息
                var article = await _fileArticleContentRepository.FirstOrDefaultAsync(r=>r.FileManageId==result.Id);
                if(article!=null)result.Article= ObjectMapper.Map<FileArticleContentInfoDto>(article);
                //获取文件明细数据
                var files = await _fileListsRepository.GetAllListAsync(r =>r.FileManageId==result.Id);
                if(files!=null && files.Count<=0)result.FileData= ObjectMapper.Map<List<FileListsDto>>(files);
                return result;
            }
            catch (Exception ex)
            {
                throw new AbpException(message: $"根据id获取文件/档案信息发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除附件文件
        /// </summary>
        /// <param name="id">唯一标识id</param>
        /// <returns></returns>
        public async Task DeleteFileAsync(long id)
        {
            try
            {
                var fileInfo = await _fileListsRepository.FirstOrDefaultAsync(id);
                if (fileInfo == null) throw new UserFriendlyException(L("找不到附件文件信息！"));
                //根据文件绝对路径判断文件是否存在，存在删除
                if (File.Exists(fileInfo.FinalyFilePath))
                    File.Delete(fileInfo.FinalyFilePath);
                await _fileListsRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                throw new AbpException(message: $"删除附件文件发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取文件文件信息
        /// </summary>
        /// <param name="id">唯一标识id</param>
        /// <returns>Item1.附件文件信息 Item2.文件绝对路径</returns>
        public async Task<(FileListsDto,string)> GetFileInfoAsync(long id)
        {
            try
            {
                var fileInfo = await _fileListsRepository.FirstOrDefaultAsync(id);
                if (fileInfo == null) return (null,null);
                var result= ObjectMapper.Map<FileListsDto>(fileInfo);
                return (result,fileInfo.FinalyFilePath);
            }
            catch (Exception ex)
            {
                throw new AbpException(message: $"获取文件文件信息发生异常：{ex.Message}");
            }
        }

    }
}
