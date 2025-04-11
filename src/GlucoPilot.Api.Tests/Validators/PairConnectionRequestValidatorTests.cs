using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.LibreLink.PairConnection;
using NUnit.Framework;
using System;

namespace GlucoPilot.Tests.Endpoints.LibreLink.PairConnection;

public class PairConnectionRequestValidatorTests
{
    private readonly PairConnectionRequest.PairConnectionRequestValidator _validator;

    public PairConnectionRequestValidatorTests()
    {
        _validator = new PairConnectionRequest.PairConnectionRequestValidator();
    }

    [Test]
    public void Should_HaveValidationError_When_PatientIdIsEmpty()
    {
        var request = new PairConnectionRequest
        {
            PatientId = Guid.Empty
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Test]
    public void Should_NotHaveValidationError_When_PatientIdIsValid()
    {
        var request = new PairConnectionRequest
        {
            PatientId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.PatientId);
    }
}