using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Endpoints.Injections
{
    [ExcludeFromCodeCoverage]
    public static class InjectionsEndpoints
    {
        internal static IEndpointRouteBuilder MapInjectionsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/injections")
                .WithTags("Injections");

            return endpoints;
        }
    }
}
