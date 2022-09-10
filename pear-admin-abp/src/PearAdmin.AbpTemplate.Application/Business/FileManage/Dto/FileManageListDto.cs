using Shipeng.Util;

namespace PearAdmin.AbpTemplate.Business.FileManage.Dto
{
    /// <summary>
    /// 文件管理/档案管理数据传输模型
    /// </summary>
    public class FileManageListDto: FileManageEntity
    {
        /// <summary>
        /// 创建人
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// 文件公开类型(0.完全公开 1.部门内部公开 2.部门内部及以下公开 3.仅自己可见)
        /// </summary>
        public string FilePublicTypeText => FilePublicType.GetDescription();

        /// <summary>
        /// 分类名称
        /// </summary>
        public string ClassifyName { get; set; }
    }
}
