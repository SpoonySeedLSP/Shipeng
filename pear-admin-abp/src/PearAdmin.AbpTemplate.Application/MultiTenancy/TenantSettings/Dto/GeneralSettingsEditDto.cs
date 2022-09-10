namespace PearAdmin.AbpTemplate.MultiTenancy.TenantSetting.Dto
{
    public class GeneralSettingsEditDto
    {
        /// <summary>
        /// 时区
        /// </summary>
        public string Timezone { get; set; }

        /// <summary>
        ///  此值仅用于比较用户的时区和默认时区
        /// </summary>
        public string TimezoneForComparison { get; set; }
    }
}