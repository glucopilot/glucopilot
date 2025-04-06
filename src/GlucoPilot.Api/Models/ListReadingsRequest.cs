using FluentValidation;
using System;
using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Api.Models
{
    public sealed record ListReadingsRequest
    {
        [Required]
        public DateTimeOffset From { get; set; }

        [Required]
        public DateTimeOffset? To { get; set; }
    }

    public sealed class Validator : AbstractValidator<ListReadingsRequest>
    {
        public Validator()
        {
            RuleFor(x => x.From).LessThan(x => x.To);
            RuleFor(x => x.To).LessThanOrEqualTo(DateTimeOffset.UtcNow);
        }
    }
}
