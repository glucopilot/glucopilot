﻿using FluentValidation;
using System;
using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Api.Endpoints.Readings.List;

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
        RuleFor(x => x.From).LessThan(x => x.To).WithMessage(Resources.ValidationMessages.ToBeforeFrom);
        RuleFor(x => x.MinuteInterval).GreaterThan(0).WithMessage(Resources.ValidationMessages.IntervalInvalid).LessThan(60).WithMessage(Resources.ValidationMessages.IntervalInvalid);
    }
}