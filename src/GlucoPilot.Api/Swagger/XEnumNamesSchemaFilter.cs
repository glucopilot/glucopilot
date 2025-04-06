using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GlucoPilot.Api.Swagger;

[ExcludeFromCodeCoverage]
public sealed class XEnumNamesSchemaFilter : ISchemaFilter
{
    private const string Name = "x-enumNames";

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var typeInfo = context.Type;

        if (typeInfo.IsEnum && !schema.Extensions.ContainsKey(Name))
        {
            var names = Enum.GetNames(context.Type);
            var arr = new OpenApiArray();
            arr.AddRange(names.Select(name => new OpenApiString(name)));

            schema.Enum = arr;
            schema.Type = "string";
            schema.Format = "string";
        }
    }
}