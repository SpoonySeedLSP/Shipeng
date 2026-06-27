using Shipeng.Dependency;
using Shipeng.DynamicApiController;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Shipeng.SpecificationDocument
{
    /// <summary>
    /// ±Í«©Œƒµµ≈≈–Ú¿πΩÿ∆˜
    /// </summary>
    [SuppressSniffer]
    public class TagsOrderDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// ≈‰÷√¿πΩÿ
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = swaggerDoc.Tags
                                        .OrderByDescending(u => GetTagOrder(u.Name))
                                        .ThenBy(u => u.Name)
                                        .ToHashSet();
        }

        /// <summary>
        /// ªÒ»°±Í«©≈≈–Ú
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private static int GetTagOrder(string tag)
        {
            var isExist = Penetrates.ControllerOrderCollection.TryGetValue(tag, out var order);
            return isExist ? order : default;
        }
    }
}
