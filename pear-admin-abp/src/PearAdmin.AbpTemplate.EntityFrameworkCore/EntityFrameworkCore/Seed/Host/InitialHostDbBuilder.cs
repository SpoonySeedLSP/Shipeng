using PearAdmin.AbpTemplate.EntityFrameworkCore.Seed.Common;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.Seed.Host
{
    /// <summary>
    /// 初始化宿主Db构建器
    /// </summary>
    public class InitialHostDbBuilder
    {
        private readonly AbpTemplateDbContext _context;

        public InitialHostDbBuilder(AbpTemplateDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            new DefaultEditionCreator(_context).Create();//默认的版本的创造者
            new HostRoleAndUserCreator(_context).Create();//宿主角色和用户创建者
            new DefaultLanguagesCreator(_context).Create();//默认语言的创造者
            new DefaultSettingsCreator(_context).Create();//默认设置的创造者

            _context.SaveChanges();
        }
    }
}
