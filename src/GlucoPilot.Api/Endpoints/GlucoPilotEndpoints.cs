using GlucoPilot.Api.Endpoints.Readings;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints;

[ExcludeFromCodeCoverage]
public static class GlucoPilotEndpoints
{
    internal static IEndpointRouteBuilder MapGlucoPilotEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapReadingsEndpoints();

        return endpoints;
    }
}
