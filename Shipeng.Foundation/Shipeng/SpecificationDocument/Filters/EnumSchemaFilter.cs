using Shipeng.Dependency;
using Shipeng.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Shipeng.SpecificationDocument
{
    /// <summary>
    /// 修正 规范化文�?Enum 提示
    /// </summary>
    [SuppressSniffer]
    public class EnumSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// 实现过滤器方�?
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        public void Apply(IOpenApiSchema model, SchemaFilterContext context)
        {
            var type = context.Type;

            // 排除其他程序集的枚举
            if (type.IsEnum && App.Assemblies.Contains(type.Assembly))
            {
                /* 新版 OpenApi 规则：Enum集合�?OpenApi 框架内部自动构造，业务代码不能手动 new 覆盖
                 * 极端场景model.Enum==null：直接return放弃填充枚举值，避免报错，不影响文档整体加载
                 */
                if (model.Enum is null) return;
                model.Enum.Clear();
                var stringBuilder = new StringBuilder();
                stringBuilder.Append($"{model.Description}<br />");

                var enumValues = Enum.GetValues(type);
                // 获取枚举实际值类�?
                var enumValueType = type.GetFields().First().FieldType;

                foreach (var value in enumValues)
                {
                    var numValue = value.ChangeType(enumValueType);

                    // 获取枚举成员特�?
                    var fieldinfo = type.GetField(Enum.GetName(type, value));
                    var descriptionAttribute = fieldinfo.GetCustomAttribute<DescriptionAttribute>(true);
                    //model.Enum.Add(OpenApiAnyFactory.CreateFromJson($"{numValue}"));
                    model.Enum.Add(JsonValue.Create(numValue));//.NET10无OpenApiAnyFactory，替换为框架原生JsonValue

                    stringBuilder.Append($"&nbsp;{descriptionAttribute?.Description} {value} = {numValue}<br />");
                }
                model.Description = stringBuilder.ToString();
            }
        }
    }
}
