using System.Collections.Generic;

namespace PearAdmin.AbpTemplate.Business.FileManage.Dto
{
    /// <summary>
    /// 文件/档案信息传输模型
    /// </summary>
    public class FileManageInfoDto: FileManageListDto
    {
        /// <summary>
        /// 文件文章信息
        /// </summary>
        public FileArticleContentInfoDto Article { get; set; }

        /// <summary>
        /// 附件文件数据
        /// </summary>
        public List<FileListsDto> FileData { get; set; }
    }
}
