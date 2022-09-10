namespace PearAdmin.AbpTemplate.Admin.Models.FileManage
{
    /// <summary>
    /// 分片文件信息
    /// </summary>
    public class SliceFileInfo
    {
        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 当前分块序号
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 所有块数
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 分片文件排序
    /// </summary>
    public class FileSort
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分片序号
        /// </summary>
        public int NumBer { get; set; }
    }
}
