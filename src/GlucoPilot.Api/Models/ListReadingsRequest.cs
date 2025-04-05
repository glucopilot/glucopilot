using FluentValidation;
using System;
using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Api.Models
{
    public sealed record ListReadingsRequest
    {
        [Required]
        public DateTimeOffset from { get; set; }

        [Required]
        public DateTimeOffset? to { get; set; }
    }

    public sealed class Validator : AbstractValidator<ListReadingsRequest>
    {
        public Validator()
        {
            RuleFor(x => x.from).LessThan(x => x.to);
            RuleFor(x => x.to).LessThanOrEqualTo(DateTimeOffset.UtcNow);
        }
    }
}
