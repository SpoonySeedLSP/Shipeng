using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using PearAdmin.AbpTemplate.Storage.Minio;

namespace PearAdmin.AbpTemplate
{
    [DependsOn(
        typeof(AbpTemplateCoreModule),
        typeof(AbpTemplateMinioStorageModule),
        typeof(AbpAutoMapperModule))]
    public class AbpTemplateApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {

        }

        public override void Initialize()
        {
            var thisAssembly = typeof(AbpTemplateApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                //扫描程序集查找继承自AutoMapper的类  配置文件
                cfg => cfg.AddMaps(thisAssembly)
            );
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}
