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
    }

    public sealed class ListReadingValidator : AbstractValidator<ListReadingsRequest>
    {
        public ListReadingValidator()
        {
            RuleFor(x => x.From).LessThan(x => x.To);
        }
    }
}
