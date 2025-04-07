﻿using FluentValidation;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Api.Models;

public sealed record ListMealsRequest
{
    [Required]
    public int Page { get; set; }

    [Required]
    public int PageSize { get; set; }

    public sealed class ListMealsValidator : AbstractValidator<ListMealsRequest>
    {
        public ListMealsValidator(IOptions<ApiSettings> apiSettings)
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, apiSettings.Value.MaxPageSize);
        }
    }
}
