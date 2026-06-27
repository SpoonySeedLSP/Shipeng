namespace Shipeng.Util
{
    /// <summary>
    /// 请求限制实体
    /// </summary>
    public class RequestRestrictionDTO
    {
        /// <summary>
        ///是否开启
        /// </summary>
        public bool IsOpen { get; set; } = false;

        /// <summary>
        /// 一秒钟(秒)
        /// </summary>
        public int LongTime { get; set; } = 60;

        /// <summary>
        /// true:请求频率设置到接口, false:所以请求
        /// </summary>
        public bool IsInterface { get; set; } = false;

        /// <summary>
        /// 可以最打请求次数
        /// </summary>
        public int Quantity { get; set; } = 500;

        /// <summary>
        /// 是否开启访问禁止
        /// </summary>
        public bool IsForbid { get; set; } = false;

        /// <summary>
        /// 封禁时长(秒)
        /// </summary>
        public int ClosingTime { get; set; } = 60;
    }
}