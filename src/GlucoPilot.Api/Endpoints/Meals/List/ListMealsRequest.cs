using FluentValidation;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Meals.List;

public sealed record ListIngredientsRequest : PagedRequest
{
    public sealed class ListMealsValidator : AbstractValidator<ListIngredientsRequest>
    {
        public ListMealsValidator(IOptions<ApiSettings> apiSettings)
        {
            Include(new PagedRequestValidator(apiSettings));
        }
    }
}
