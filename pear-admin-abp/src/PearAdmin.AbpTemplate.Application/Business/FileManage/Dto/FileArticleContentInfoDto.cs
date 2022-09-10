using Abp.Application.Services.Dto;

namespace PearAdmin.AbpTemplate.Business.FileManage.Dto
{
    /// <summary>
    /// 文件文章信息传输模型
    /// </summary>
    public class FileArticleContentInfoDto: EntityDto
    {
        /// <summary>
        /// 文章内容
        /// </summary>
        public string ArticleContent { get; set; }
    }
}
