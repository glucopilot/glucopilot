using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GlucoPilot.Api.Endpoints.Readings
{
    public static class ReadingsEndpoints
    {
        internal static IEndpointRouteBuilder MapReadingsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("readings")
                .WithTags("Readings");

            group.MapGet("/list", List.HandleAsync)
                .HasApiVersion(1.0);

            return endpoints;
        }
    }
}
