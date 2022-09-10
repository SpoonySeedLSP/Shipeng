using Abp.Application.Services.Dto;
using System;

namespace PearAdmin.AbpTemplate.Business.FileManage.Dto
{
    /// <summary>
    /// 附件文件数据传输模型
    /// </summary>
    public class FileListsDto: EntityDto
    {
        /// <summary>
        /// 文件id
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 存储文件名称
        /// </summary>
        public string StorageFileName { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 二进制数据
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// 文件存储相对目录
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 文件相对路径
        /// </summary>
        public string FilePath { get; set; }
    }
}
