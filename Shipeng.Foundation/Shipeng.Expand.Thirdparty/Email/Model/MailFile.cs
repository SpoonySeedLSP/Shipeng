using Shipeng.Dependency;
using System;

namespace Shipeng.Expand.Thirdparty.Email.Model
{
    [SuppressSniffer]
    public class MailFile
    {
        /// <summary>
        /// 文件id
        /// </summary>
        public string FileId { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public string FileSize { get; set; }
        /// <summary>
        /// 文件时间
        /// </summary>
        public DateTime FileTime { get; set; }
        /// <summary>
        /// 文件状态
        /// </summary>
        public string FileState { get; set; }
    }
}
