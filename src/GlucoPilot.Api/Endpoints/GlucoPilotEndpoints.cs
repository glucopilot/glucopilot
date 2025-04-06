using GlucoPilot.Api.Endpoints.Readings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints;

[ExcludeFromCodeCoverage]
public static class GlucoPilotEndpoints
{
    internal static IEndpointRouteBuilder MapGlucoPilotEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/glucopilot")
            .WithTags("GlucoPilot");

        group.MapReadingsEndpoints();

        return endpoints;
    }
}
