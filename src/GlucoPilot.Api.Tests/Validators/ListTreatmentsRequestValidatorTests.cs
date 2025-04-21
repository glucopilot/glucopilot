using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Treatments.ListTreatments;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
public class ListTreatmentsRequestValidatorTests
{
    private ListTreatmentsRequest.ListTreatmentsRequestValidator _validator;

    [SetUp]
    public void SetUp()
    {
        var apiSettings = Options.Create(new ApiSettings { MaxPageSize = 25 });
        _validator = new ListTreatmentsRequest.ListTreatmentsRequestValidator(apiSettings);
    }

    [Test]
    public void Should_Not_Have_Error_When_To_Is_Null()
    {
        var request = new ListTreatmentsRequest
        {
            From = DateTimeOffset.Now.AddDays(-1),
            To = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r.From);
    }

    [Test]
    public void Should_Not_Have_Error_When_To_Is_Null_And_From_Is_In_Future()
    {
        var request = new ListTreatmentsRequest
        {
            From = DateTimeOffset.Now.AddDays(1),
            To = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r.From);
    }

    [Test]
    public void Should_Have_Error_When_From_Is_After_To()
    {
        var request = new ListTreatmentsRequest
        {
            From = DateTimeOffset.Now.AddDays(-1),
            To = DateTimeOffset.Now.AddDays(-2)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r)
            .WithErrorMessage("TO_BEFORE_FROM");
    }

    [Test]
    public void Should_Not_Have_Error_When_From_Is_Before_To()
    {
        var request = new ListTreatmentsRequest
        {
            From = DateTimeOffset.Now.AddDays(-2),
            To = DateTimeOffset.Now.AddDays(-1)
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r);
    }

    [Test]
    public void Should_Not_Have_Error_When_Both_From_And_To_Are_Null()
    {
        var request = new ListTreatmentsRequest
        {
            From = null,
            To = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r);
    }
}
