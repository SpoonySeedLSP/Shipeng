using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace PearAdmin.AbpTemplate.Business.FileManage
{
    /// <summary>
    /// 文件文章信息
    /// </summary>
    public class FileArticleContentEntity : AggregateRoot<long>, IHasCreationTime, IMayHaveTenant
    {
        /// <summary>
        /// 文章内容最大存储大小
        /// </summary>
        public const int MaxArticleContentLength = 10 * 024 * 1024; //10MB

        /// <summary>
        /// 创建人id
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// 宿主id
        /// </summary>
        public int? HostId { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// 企业id
        /// </summary>
        public long EnterpriseId { get; set; }

        /// <summary>
        /// 组织id
        /// </summary>
        public long OrganizationId { get; set; }

        /// <summary>
        /// 文件/档案id
        /// </summary>
        public long FileManageId { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        [Required]
        public long ClassifyId { get; set; }

        /// <summary>
        /// 文章内容
        /// </summary>
        public string ArticleContent { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
