using FluentValidation.TestHelper;
using GlucoPilot.Identity.Models;

namespace GlucoPilot.Identity.Tests.Validators;

[TestFixture]
internal sealed class RevokeTokenRequestValidatorTests
{
    private RevokeTokenRequest.Validator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new RevokeTokenRequest.Validator();
    }

    [Test]
    public void Should_Have_Error_When_Token_Is_Empty()
    {
        var model = new RevokeTokenRequest
        {
            Token = string.Empty,
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Test]
    public void Should_Have_Error_When_Token_Is_Null()
    {
        var model = new RevokeTokenRequest
        {
            Token = null,
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }
}