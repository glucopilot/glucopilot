using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GlucoPilot.Api.Swagger;

[ExcludeFromCodeCoverage]
public sealed class XEnumNamesSchemaFilter : ISchemaFilter
{
    private const string Name = "x-enumNames";

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema openApiSchema)
        {
            var typeInfo = context.Type;

            if (!typeInfo.IsEnum || (openApiSchema.Extensions?.ContainsKey(Name) ?? false))
            {
                return;
            }
            
            var names = Enum.GetNames(context.Type);
            var arr = new JsonArray();
            foreach (var name in names)
            {
                arr.Add(name);
            }

            openApiSchema.Enum = arr.ToList()!;
            openApiSchema.Type = JsonSchemaType.String;
            openApiSchema.Format = null;
        }
    }
}