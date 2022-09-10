using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PearAdmin.AbpTemplate.Business.Enterprise;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.EntityTypeConfigurations
{
    /// <summary>
    /// 企业信息实体类型配置
    /// </summary>
    public class EnterpriseTypeConfiguration : IEntityTypeConfiguration<EnterpriseEntity>
    {
        public void Configure(EntityTypeBuilder<EnterpriseEntity> builder)
        {
            builder.ToTable($"{AbpTemplateCoreConsts.DbTablePrefix}_Enterprise");

            /* 返回可用于配置实体类型属性的对象
             * 如果指定的属性还不是模型的一部分，它将被添加
             * 参数:
             *     propertyExpression:一个lambda表达式，表示要配置的属性( blog => blog.Url)
             * 返回结果:An object that can be used to configure the property.
             */
            builder.Property(a => a.CompanyInformation)
                .HasMaxLength(EnterpriseEntity.MaxInfoLength)
                .IsRequired(false);
        }
    }
}
