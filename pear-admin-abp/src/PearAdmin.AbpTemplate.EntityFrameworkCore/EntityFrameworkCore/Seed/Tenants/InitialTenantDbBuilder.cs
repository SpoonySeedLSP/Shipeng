using Abp.MultiTenancy;
using PearAdmin.AbpTemplate.EntityFrameworkCore.Seed.Common;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.Seed.Tenants
{
    /// <summary>
    /// 初始租户Db构建器
    /// </summary>
    public class InitialTenantDbBuilder
    {
        private readonly AbpTemplateDbContext _context;

        public InitialTenantDbBuilder(AbpTemplateDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            //默认租户初始化(Id为1)
            DefaultTenant();

            _context.SaveChanges();
        }

        private void DefaultTenant()
        {
            new DefaultTenantBuilder(_context).Create();//默认的租户构建器
            new TenantRoleAndUserBuilder(_context, MultiTenancyConsts.DefaultTenantId).Create();//租户角色和用户构建器
            new DefaultLanguagesCreator(_context, MultiTenancyConsts.DefaultTenantId).Create();//默认语言的创造者
            new DefaultSettingsCreator(_context, MultiTenancyConsts.DefaultTenantId).Create();//默认设置的创造者
            new TenantSettingsCreator(_context, MultiTenancyConsts.DefaultTenantId).Create();//租户设置创造者
        }
    }
}
