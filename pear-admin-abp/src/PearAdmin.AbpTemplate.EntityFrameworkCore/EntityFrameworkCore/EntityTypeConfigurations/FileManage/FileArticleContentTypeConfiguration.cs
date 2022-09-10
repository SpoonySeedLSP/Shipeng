using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PearAdmin.AbpTemplate.Business.FileManage;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.EntityTypeConfigurations
{
    /// <summary>
    /// 文件文章信息实体模型映射
    /// </summary>
    public class FileArticleContentTypeConfiguration : IEntityTypeConfiguration<FileArticleContentEntity>
    {
        public void Configure(EntityTypeBuilder<FileArticleContentEntity> builder)
        {
            builder.ToTable($"{AbpTemplateCoreConsts.DbTablePrefix}_FileArticleContent");

            /* 返回可用于配置实体类型属性的对象
             * 如果指定的属性还不是模型的一部分，它将被添加
             * 参数:
             *     propertyExpression:一个lambda表达式，表示要配置的属性( blog => blog.Url)
             * 返回结果:An object that can be used to configure the property.
             */
            builder.Property(a => a.ArticleContent)
                .HasMaxLength(FileArticleContentEntity.MaxArticleContentLength)
                .IsRequired(false);
        }
    }
}
