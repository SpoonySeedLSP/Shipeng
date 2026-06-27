using Shipeng;
using Shipeng.Dependency;
using Shipeng.Reflection;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// ӳ����չ��
    /// </summary>
    [SuppressSniffer]
    public static class MapperServiceCollectionExtensions
    {
        /// <summary>
        /// ��Ӷ���ӳ��
        /// </summary>
        /// <param name="services">���񼯺�</param>
        /// <returns></returns>
        public static IServiceCollection AddObjectMapper(this IServiceCollection services)
        {
            // �ж��Ƿ�װ�� Mapster ����
            var mapperAssembly = App.Assemblies.FirstOrDefault(u => u.GetName().Name.Equals("Shipeng.Mapster"));
            if (mapperAssembly != null)
            {
                // ���� Mapper ��չ���ͺ���չ����
                var objectMapperServiceCollectionExtensionsType = Reflect.GetType(mapperAssembly, $"Microsoft.Extensions.DependencyInjection.ObjectMapperServiceCollectionExtensions");
                var addObjectMapperMethod = objectMapperServiceCollectionExtensionsType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(u => u.Name == "AddObjectMapper");

                return addObjectMapperMethod.Invoke(null, new object[] { services, App.Assemblies.ToArray() }) as IServiceCollection;
            }

            return services;
        }
    }
}