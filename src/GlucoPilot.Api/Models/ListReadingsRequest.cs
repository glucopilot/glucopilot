using FluentValidation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Api.Models
{
    public sealed record ListReadingsRequest
    {
        [Required]
        public DateTimeOffset From { get; set; }

        [Required]
        public DateTimeOffset? To { get; set; }
    }

    public sealed class ListReadingValidator : AbstractValidator<ListReadingsRequest>
    {
        public ListReadingValidator()
        {
            RuleFor(x => x.From).LessThan(x => x.To);
            RuleFor(x => x.To).LessThanOrEqualTo(DateTimeOffset.UtcNow);
        }
    }
}
