using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Injections.ListInjections;

public sealed record ListInjectionsRequest : PagedRequest
{
    public sealed class ListInjectionsValidator : PagedRequestValidator<ListInjectionsRequest>
    {
        public ListInjectionsValidator(IOptions<ApiSettings> apiSettings) : base(apiSettings)
        {
        }
    }
}