using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Treatments.ListTreatments;

public sealed record ListTreatmentsRequest : PagedRequest
{
    public sealed class ListTreatmentsRequestValidator : PagedRequestValidator<ListTreatmentsRequest>
    {
        public ListTreatmentsRequestValidator(IOptions<ApiSettings> apiSettings) : base(apiSettings)
        {
        }
    }
}