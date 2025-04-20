using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;

namespace GlucoPilot.Api.Endpoints.Sensors.List;

public sealed record ListSensorsRequest : PagedRequest
{
    public sealed class  ListSensorsValidator : PagedRequestValidator<ListSensorsRequest>
    {
        public ListSensorsValidator(IOptions<ApiSettings> apiSettings) : base(apiSettings)
        {
        }
    }
}
