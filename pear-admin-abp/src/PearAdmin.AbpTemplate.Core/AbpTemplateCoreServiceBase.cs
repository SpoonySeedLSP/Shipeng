using Abp.Domain.Services;

namespace PearAdmin.AbpTemplate
{
    /// <summary>
    /// DomainService:这个类可以用作域服务的基类
    /// </summary>
    public class AbpTemplateCoreServiceBase : DomainService
    {
        protected AbpTemplateCoreServiceBase()
        {
            LocalizationSourceName = AbpTemplateCoreConsts.LocalizationSourceName;
        }
    }
}
