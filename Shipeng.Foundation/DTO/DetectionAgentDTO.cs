namespace Shipeng.Util
{
    public class DetectionAgentDTO
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 是否跳转
        /// </summary>
        public bool IsJump { get; set; }

        /// <summary>
        /// 跳转小程序
        /// </summary>
        public string JumpAppId { get; set; }

        /// <summary>
        /// 状态吗
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 跳转路径
        /// </summary>
        public string JumpUrl { get; set; }
    }

}
