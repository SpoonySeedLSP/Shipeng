using Abp.Dependency;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.TaskCenter.AntiCorruption
{
    /// <summary>
    ///ITransientDependency: 所有实现此接口的类都会自动注册到依赖项
    /// 作为瞬时对象的注入
    /// </summary>
    public interface IOrganizationService : ITransientDependency
    {
        Task GetOrganizationListAsync();
    }
}
