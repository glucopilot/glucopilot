using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace GlucoPilot.Api.Models.Tests;

[TestFixture]
public class ListMealsValidatorTests
{
    private readonly ListMealsRequest.ListMealsValidator _validator;

    public ListMealsValidatorTests()
    {
        var apiSettings = Options.Create(new ApiSettings { MaxPageSize = 100 });
        _validator = new ListMealsRequest.ListMealsValidator(apiSettings);
    }

    [Test]
    public void Should_Have_Error_When_Page_Is_Less_Than_Zero()
    {
        var model = new ListMealsRequest { Page = -1, PageSize = 10 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Test]
    public void Should_Not_Have_Error_When_Page_Is_Zero_Or_Greater()
    {
        var model = new ListMealsRequest { Page = 0, PageSize = 10 };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Page);
    }

    [Test]
    public void Should_Have_Error_When_PageSize_Is_Less_Than_One()
    {
        var model = new ListMealsRequest { Page = 0, PageSize = 0 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Test]
    public void Should_Have_Error_When_PageSize_Is_Greater_Than_MaxPageSize()
    {
        var model = new ListMealsRequest { Page = 0, PageSize = 101 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Test]
    public void Should_Not_Have_Error_When_PageSize_Is_Within_Valid_Range()
    {
        var model = new ListMealsRequest { Page = 0, PageSize = 50 };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }
}