using FluentValidation;
using GlucoPilot.Api.Endpoints.Meals.List;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Ingredients.List;

public sealed record ListIngredientsRequest : PagedRequest
{
    public sealed class ListIngredientsValidator : AbstractValidator<ListIngredientsRequest>
    {
        public ListIngredientsValidator(IOptions<ApiSettings> apiSettings)
        {
            Include(new PagedRequestValidator(apiSettings));
        }
    }
}
