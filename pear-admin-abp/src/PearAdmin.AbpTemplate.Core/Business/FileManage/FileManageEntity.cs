using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using PearAdmin.AbpTemplate.Common.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace PearAdmin.AbpTemplate.Business.FileManage
{
    /// <summary>
    /// 文件管理/档案管理信息
    /// </summary>
    public class FileManageEntity : AggregateRoot<long>, IHasCreationTime, IMayHaveTenant
    {
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
        /// 文件公开类型(0.完全公开 1.部门内部公开 2.仅自己可见)
        /// </summary>
        public FilePublicTypeEnum FilePublicType { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        [Required]
        public long ClassifyId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(2000)]
        public string Describe { get; set; }

        /// <summary>
        /// 浏览次数
        /// </summary>
        [Range(0,int.MaxValue)]
        public int Viewed { get; set; }

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
