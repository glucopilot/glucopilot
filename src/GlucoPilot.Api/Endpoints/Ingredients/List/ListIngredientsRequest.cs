using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Ingredients.List;

public sealed record ListIngredientsRequest : PagedRequest
{
    public sealed class ListIngredientsValidator : PagedRequestValidator<ListIngredientsRequest>
    {
        public ListIngredientsValidator(IOptions<ApiSettings> apiSettings) : base(apiSettings)
        {
        }
    }
}
