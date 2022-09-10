using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace PearAdmin.AbpTemplate.Business.FileManage
{
    /// <summary>
    /// 文件明细信息
    /// </summary>
    public class FileListsEntity : AggregateRoot<long>, IHasCreationTime, IMayHaveTenant
    {
        /// <summary>
        /// 文件二进制数据最大存储大小
        /// </summary>
        public const int MaxBytesLength = 10 * 024 * 1024; //10MB

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
        /// 文件id
        /// </summary>
        [Required]
        public string FileId { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        [Required]
        public string ContentType { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        [Required]
        public string FileName { get; set; }

        /// <summary>
        /// 存储文件名称
        /// </summary>
        [Required]
        public string StorageFileName { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        [Required]
        public long FileSize { get; set; }

        /// <summary>
        /// 二进制数据
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// 文件存储绝对目录
        /// </summary>
        [Required]
        public string DirectoryPath { get; set; }

        /// <summary>
        /// 文件存储相对目录
        /// </summary>
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// 文件相对路径
        /// </summary>
        [Required]
        public string FilePath { get; set; }

        /// <summary>
        /// 文件绝对路径
        /// </summary>
        public string FinalyFilePath { get; set; }

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
