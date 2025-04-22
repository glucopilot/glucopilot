using FluentValidation;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Meals.List;

public sealed record ListMealsRequest : PagedRequest
{
    public string? Search { get; set; }

    public sealed class ListMealsValidator : PagedRequestValidator<ListMealsRequest>
    {     
        public ListMealsValidator(IOptions<ApiSettings> apiSettings) : base(apiSettings)
        {
            RuleFor(x => x.Search)
            .MinimumLength(3)
            .When(x => !string.IsNullOrEmpty(x.Search))
            .WithMessage(Resources.ValidationMessages.SearchLengthInvalid);
        }
    }
}
