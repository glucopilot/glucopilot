using FluentValidation;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Ingredients.List;

public sealed record ListIngredientsRequest : PagedRequest
{
    public string? Search { get; set; } = null;
    public sealed class ListIngredientsValidator : PagedRequestValidator<ListIngredientsRequest>
    {
        public ListIngredientsValidator(IOptions<ApiSettings> apiSettings) : base(apiSettings)
        {
            RuleFor(x => x.Search)
            .MinimumLength(3)
            .When(x => !string.IsNullOrEmpty(x.Search))
            .WithMessage(Resources.ValidationMessages.SearchLengthInvalid);
        }
    }
}
