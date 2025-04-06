﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GlucoPilot.Api.Endpoints.Readings
{
    public static class ReadingsEndpoints
    {
        internal static IEndpointRouteBuilder MapReadingsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.NewVersionedApi().MapGroup("api/v{version:apiVersion}/readings")
                .WithTags("Readings");

            group.MapGet("/", List.HandleAsync)
                .HasApiVersion(1.0)
                .RequireAuthorization();

            return endpoints;
        }
    }
}
