using FluentValidation;
using GlucoPilot.Api.Endpoints.Meals.List;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Injections.ListInjections;

public sealed record ListInjectionsRequest : PagedRequest
{
    public sealed class ListInjectionsValidator : AbstractValidator<ListMealsRequest>
    {
        public ListInjectionsValidator(IOptions<ApiSettings> apiSettings)
        {
            Include(new PagedRequestValidator(apiSettings));
        }
    }
}