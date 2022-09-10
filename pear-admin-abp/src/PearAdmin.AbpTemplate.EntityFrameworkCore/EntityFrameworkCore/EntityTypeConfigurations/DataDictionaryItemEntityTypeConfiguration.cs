using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PearAdmin.AbpTemplate.Resource.DataDictionaries;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.EntityTypeConfigurations
{
    /// <summary>
    /// 数据字典项信息实体模型映射
    /// </summary>
    public class DataDictionaryItemEntityTypeConfiguration : IEntityTypeConfiguration<DataDictionaryItem>
    {
        public void Configure(EntityTypeBuilder<DataDictionaryItem> builder)
        {
            builder.ToTable($"{AbpTemplateCoreConsts.TablePrefix_Resource}_DataDictionaryItem");

            builder.Property(b => b.Code)
                .HasMaxLength(DataDictionaryItem.MaxCodeLength)
                .IsRequired();

            builder.Property(b => b.Name)
                .HasMaxLength(DataDictionaryItem.MaxNameLength)
                .IsRequired();

            builder.Property(b => b.Describe)
                .HasMaxLength(DataDictionaryItem.MaxDescribeLength)
                .IsRequired(false);
        }
    }
}
