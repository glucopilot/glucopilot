using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints.Pens;

[ExcludeFromCodeCoverage]
public static class PensEndpoints
{
    internal static IEndpointRouteBuilder MapPensEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/pens")
            .WithTags("Pens");

        return endpoints;
    }
}
