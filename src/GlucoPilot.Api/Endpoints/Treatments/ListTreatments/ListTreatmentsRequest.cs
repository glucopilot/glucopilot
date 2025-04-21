using FluentValidation;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;
using System;

namespace GlucoPilot.Api.Endpoints.Treatments.ListTreatments;

public sealed record ListTreatmentsRequest : PagedRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }

    public sealed class ListTreatmentsRequestValidator : PagedRequestValidator<ListTreatmentsRequest>
    {      
        public ListTreatmentsRequestValidator(IOptions<ApiSettings> apiSettings) : base(apiSettings)
        {
            RuleFor(request => request)
                .Must(r => r.From == null || r.To == null || r.From <= r.To)
                .WithMessage(Resources.ValidationMessages.ToBeforeFrom);
        }
    }
}