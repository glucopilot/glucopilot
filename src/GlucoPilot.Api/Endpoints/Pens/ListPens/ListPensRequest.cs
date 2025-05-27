using GlucoPilot.Api.Endpoints.Meals.List;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Pens.ListPens;

public sealed record ListPensRequest : PagedRequest
{
    public sealed class ListPensValidator : PagedRequestValidator<ListMealsRequest>
    {
        public ListPensValidator(IOptions<ApiSettings> apiSettings) : base(apiSettings) { }
    }
}
