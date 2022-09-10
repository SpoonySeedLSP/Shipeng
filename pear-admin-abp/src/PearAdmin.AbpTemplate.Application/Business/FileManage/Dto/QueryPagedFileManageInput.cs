using PearAdmin.AbpTemplate.Common.Enum;
using PearAdmin.AbpTemplate.CommonDto;
using System;

namespace PearAdmin.AbpTemplate.Business.FileManage.Dto
{
    /// <summary>
    /// 分页查询文件管理/档案管理模型
    /// </summary>
    public class QueryPagedFileManageInput: PagedAndFilteredInputDto
    {
        /// <summary>
        /// 组织id
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// 文件公开类型(null.全部 0.完全公开 1.部门内部公开 2.仅自己可见)
        /// </summary>
        public FilePublicTypeEnum? FilePublicType { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        public long? ClassifyId { get; set; }

        /// <summary>
        /// 关键词模糊搜索
        /// </summary>
        public string KeyWord { get; set; }

        /// <summary>
        /// 按时间范围筛选-开始时间
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 按时间范围筛选-截止时间
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
