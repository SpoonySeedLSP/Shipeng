using AutoMapper;
using PearAdmin.AbpTemplate.Business.FileManage.Dto;

namespace PearAdmin.AbpTemplate.Business.FileManage
{
    /// <summary>
    /// 文件管理/档案管理模型映射配置文件
    /// </summary>
    public class FileManageMapperProfile : Profile
    {
        public FileManageMapperProfile()
        {
            //文件管理/档案管理信息实体模型转文件管理/档案管理数据传输模型
            CreateMap<FileManageEntity, FileManageListDto>();
            //文件管理/档案管理信息实体模型转文件/档案信息传输模型
            CreateMap<FileManageEntity, FileManageInfoDto>();
            //文件文章信息实体模型转文件文章信息传输模型
            CreateMap<FileArticleContentEntity, FileArticleContentInfoDto>();
            //文件明细信息实体模型转附件文件数据传输模型
            CreateMap<FileListsEntity, FileListsDto>();
            
        }
    }
}
