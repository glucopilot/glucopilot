using FluentValidation;
using System;
using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Api.Endpoints.Readings.List
{
    public sealed record ListReadingsRequest
    {
        [Required]
        public DateTimeOffset From { get; set; }

        [Required]
        public DateTimeOffset? To { get; set; }

        public int MinuteInterval { get; set; } = 1;
    }

    public sealed class ListReadingValidator : AbstractValidator<ListReadingsRequest>
    {
        public ListReadingValidator()
        {
            RuleFor(x => x.From).LessThan(x => x.To).WithMessage("TO_BEFORE_FROM");
            RuleFor(x => x.MinuteInterval).GreaterThan(0).WithMessage("INTERVAL_INVALID").LessThan(60).WithMessage("INTERVAL_INVALID");
        }
    }
}
