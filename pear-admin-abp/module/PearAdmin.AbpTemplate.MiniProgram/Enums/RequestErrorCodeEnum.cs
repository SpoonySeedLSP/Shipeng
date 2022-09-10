namespace PearAdmin.AbpTemplate.MiniProgram.Enums
{
    /// <summary>
    /// 请求错误状态枚举
    /// </summary>
    public enum RequestErrorCodeEnum
    {
        Busy = -1,
        Succeed = 0,
        CodeError = 40029,
        FrequencyLimit = 45011,
        HighRiskUser = 40226
    }
}
