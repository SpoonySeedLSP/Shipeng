
namespace Shipeng.Util
{
    /// <summary>
    /// 连续 GUID 配置
    /// </summary>
    public sealed class SequentialGuidSettings
    {
        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTimeOffset TimeNow { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// LittleEndianBinary 16 格式化
        /// </summary>
        public bool LittleEndianBinary16Format { get; set; } = false;
    }
}