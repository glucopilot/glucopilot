using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Insulins.List;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
public class ListInsulinsValidatorTests
{
    private readonly ListInsulinsRequest.ListInsulinsValidator _validator;

    public ListInsulinsValidatorTests()
    {
        var apiSettings = Options.Create(new ApiSettings { MaxPageSize = 100 });
        _validator = new ListInsulinsRequest.ListInsulinsValidator(apiSettings);
    }

    [Test]
    public void Should_HaveValidationError_When_PageSizeIsMissing()
    {
        var request = new ListInsulinsRequest { Page = 1 };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Test]
    public void Should_HaveValidationError_When_TypeIsInvalid()
    {
        var request = new ListInsulinsRequest
        {
            Page = 1,
            PageSize = 10,
            Type = (InsulinType)(-1)
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Test]
    public void Should_NotHaveValidationError_When_TypeIsNull()
    {
        var request = new ListInsulinsRequest
        {
            Page = 1,
            PageSize = 10,
            Type = null
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Test]
    public void Should_NotHaveValidationError_When_TypeIsValid()
    {
        var request = new ListInsulinsRequest
        {
            Page = 1,
            PageSize = 10,
            Type = InsulinType.Bolus
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }
}