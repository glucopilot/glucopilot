using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GlucoPilot.Api.Swagger;

[ExcludeFromCodeCoverage]
public class SecurityRequirementsOperationFilter : IOperationFilter
{
    private readonly AuthorizationOptions _authorizationOptions;

    public SecurityRequirementsOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
    {
        // Beware: This might only part of the truth. If someone exchanges the IAuthorizationPolicyProvider and that loads
        // policies and requirements from another source than the configured options, we might not get all requirements
        // from here. But then we would have to make asynchronous calls from this synchronous interface.
        _authorizationOptions =
            authorizationOptions?.Value ?? throw new ArgumentNullException(nameof(authorizationOptions));
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var isAuthed = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any() ||
                       context.MethodInfo.GetCustomAttributes(true)
                           .Concat(context.MethodInfo.DeclaringType.GetCustomAttributes(true))
                           .OfType<AuthorizeAttribute>().Any();

        if (isAuthed)
        {
            operation.Security =
                [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("bearer", context.Document)] = [] }];
            
            _ = operation.Responses.TryAdd("401", new OpenApiResponse
            {
                Description = "Unauthorized",
                Content = new ConcurrentDictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResult), context.SchemaRepository)
                    }
                }
            });
        }
    }
}