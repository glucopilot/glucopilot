using FluentValidation;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Treatments.ListTreatments;

public sealed record ListTreatmentsRequest : PagedRequest
{
    public sealed class ListTreatmentsRequestValidator : AbstractValidator<ListTreatmentsRequest>
    {
        public ListTreatmentsRequestValidator(IOptions<ApiSettings> apiSettings)
        {
            Include(new PagedRequestValidator(apiSettings));
        }
    }
}