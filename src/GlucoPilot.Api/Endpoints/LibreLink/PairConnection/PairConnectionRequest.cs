using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.LibreLink.PairConnection;

public sealed record PairConnectionRequest
{
    public required Guid PatientId { get; set; }

    public class PairConnectionRequestValidator : AbstractValidator<PairConnectionRequest>
    {
        public PairConnectionRequestValidator()
        {
            RuleFor(x => x.PatientId).NotEmpty();
        }
    }
}
