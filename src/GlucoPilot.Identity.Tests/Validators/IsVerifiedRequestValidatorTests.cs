using FluentValidation.TestHelper;
using GlucoPilot.Identity.Models;

namespace GlucoPilot.Identity.Tests.Validators;

[TestFixture]
internal sealed class IsVerifiedRequestValidatorTests
{
    private IsVerifiedRequest.Validator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new IsVerifiedRequest.Validator();
    }

    [Test]
    public void Should_Have_Error_When_Email_IsEmpty()
    {
        var model = new IsVerifiedRequest
        {
            Email = string.Empty,
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var model = new IsVerifiedRequest
        {
            Email = "invalid-email"
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}