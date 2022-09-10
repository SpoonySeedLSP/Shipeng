using System;

namespace PearAdmin.AbpTemplate.Common.CommonDto
{
    /// <summary>
    /// 上传文件输出模型
    /// </summary>
    public class FileUploadOutputDto
    {
        /// <summary>
        /// 文件id
        /// </summary>
        public Guid FileId { get; set; }

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
        /// 文件存储相对目录
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 文件相对路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件二进制数据
        /// </summary>
        public byte[] Bytes { get; set; }
    }

    /// <summary>
    /// 分片文件上传响应模型
    /// </summary>
    public class SliceFileResponseDto
    {
        /// <summary>
        /// 当前分片文件名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// 分片文件相对路径
        /// </summary>
        public string FilePath { get; set; } = "";

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool Completed { get; set; } = false;

        /// <summary>
        /// 当前分块序号
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 所有块数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 当前分片文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 合并文件id
        /// </summary>
        public Guid? FileId { get; set; } = null;

        /// <summary>
        /// 合并后文件类型
        /// </summary>
        public string ContentType { get; set; } = "";

        /// <summary>
        /// 合并后存储文件名称
        /// </summary>
        public string StorageFileName { get; set; } = "";

        /// <summary>
        /// 合并后文件存储相对地址
        /// </summary>
        public string RelativeFilePath { get; set; } = "";

        /// <summary>
        /// 合并后文件存储相对目录
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// 合并后文件大小
        /// </summary>
        public long MergeFileSize { get; set; } = 0;
    }

}
