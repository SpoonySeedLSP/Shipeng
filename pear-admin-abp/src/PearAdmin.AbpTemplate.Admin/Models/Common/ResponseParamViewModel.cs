namespace PearAdmin.AbpTemplate.Admin.Models.Common
{
    /// <summary>
    /// 封装Layui要求的响应参数
    /// </summary>
    public class ResponseParamViewModel
    {
        public ResponseParamViewModel(string msg = "", int code = 200,bool success=true)
        {
            Code = code;
            Msg = msg;
            Success = code == 200;
        }

        /// <summary>
        /// 请求状态码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 提示消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
    }
}
