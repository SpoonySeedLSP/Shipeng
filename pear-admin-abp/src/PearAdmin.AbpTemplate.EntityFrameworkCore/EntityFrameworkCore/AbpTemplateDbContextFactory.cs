using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PearAdmin.AbpTemplate.Configuration;
using PearAdmin.AbpTemplate.Web;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore
{
    /// <summary>
    /// 只在用命令构建迁移脚本时使用
    /// https://docs.microsoft.com/zh-cn/ef/core/cli/dbcontext-creation?tabs=vs
    /// </summary>
    public class AbpTemplateDbContextFactory : IDesignTimeDbContextFactory<AbpTemplateDbContext>
    {
        public AbpTemplateDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AbpTemplateDbContext>();
            //获取配置文件信息
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder(),
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), addUserSecrets: true);
            //通过连接字符串切换对应的数据库，ConnectionStringName默认为Default
            AbpTemplateDbContextConfigurer.Configure(builder, configuration
                .GetConnectionString(AbpTemplateCoreConsts.ConnectionStringName));

            return new AbpTemplateDbContext(builder.Options);
        }
    }
}