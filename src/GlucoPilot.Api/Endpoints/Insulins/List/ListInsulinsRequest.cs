using FluentValidation;
using GlucoPilot.Api.Models;
using GlucoPilot.Data.Enums;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Insulins.List;

public sealed record ListInsulinsRequest : PagedRequest
{
    public InsulinType? Type { get; init; }

    public sealed class ListInsulinsValidator : AbstractValidator<ListInsulinsRequest>
    {
        public ListInsulinsValidator(IOptions<ApiSettings> apiSettings)
        {
            Include(new PagedRequestValidator(apiSettings));
            RuleFor(x => x.Type).IsInEnum().When(x => x.Type is not null);
        }
    }
}
