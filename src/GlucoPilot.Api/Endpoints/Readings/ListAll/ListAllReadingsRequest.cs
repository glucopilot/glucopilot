using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace GlucoPilot.Api.Endpoints.Readings.ListAll;

public record ListAllReadingsRequest
{
    [Required]
    public DateTimeOffset From { get; set; }

    [Required]
    public DateTimeOffset To { get; set; }
}

public sealed class ListAllReadingsValidator : AbstractValidator<ListAllReadingsRequest>
{
    public ListAllReadingsValidator()
    {
        RuleFor(x => x.From).LessThan(x => x.To).WithMessage(Resources.ValidationMessages.ToBeforeFrom);
    }
}