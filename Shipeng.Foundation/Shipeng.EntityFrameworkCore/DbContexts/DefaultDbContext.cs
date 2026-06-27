using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Shipeng.EntityFrameworkCore
{
    /// <summary>
    /// 默认DbContext
    /// </summary>
    public class DefaultDbContext : DbContext
    {
        /// <summary>
        /// DbContext配置
        /// </summary>
        private readonly Action<DbContextOptionsBuilder> _options;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public DefaultDbContext(Action<DbContextOptionsBuilder> options)
        {
            _options = options;
        }

        /// <summary>
        /// OnConfiguring
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _options?.Invoke(optionsBuilder);
        }

        /// <summary>
        /// OnModelCreating
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entityTypes = AssemblyHelper
                                .GetTypesFromAssembly()
                                .Where(type =>
                                    !type.Namespace.IsNullOrWhiteSpace() &&
                                    type.GetTypeInfo().IsClass &&
                                    type.GetTypeInfo().BaseType != null &&
                                    type != typeof(BaseEntity) &&
                                    typeof(BaseEntity).IsAssignableFrom(type));

            if (entityTypes != null && entityTypes.Any())
            {
                foreach (var entityType in entityTypes)
                {
                    if (modelBuilder.Model.FindEntityType(entityType) != null)
                        continue;

                    modelBuilder.Model.AddEntityType(entityType);
                }
            }
        }
    }
}
