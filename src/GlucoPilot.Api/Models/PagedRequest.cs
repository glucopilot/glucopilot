using FluentValidation;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Api.Models;

public record PagedRequest
{
    [Required]
    public int Page { get; set; }

    [Required]
    public int PageSize { get; set; }

    public class PagedRequestValidator<T> : AbstractValidator<T> where T : PagedRequest
    {
        public PagedRequestValidator(IOptions<ApiSettings> apiSettings)
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, apiSettings.Value.MaxPageSize);
        }
    }
}
