namespace Shipeng.Domain.Authorization
{
    /// <summary>
    /// jwtToken验证结果
    /// </summary>
    public class JwtTokenValidationResult
    {
        /// <summary>
        /// 验证是否成功
        /// </summary>
        public bool Successed { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 声明信息(验证成功时返回)
        /// </summary>
        public List<ClaimModel>? Claims { get; set; }
    }
}
