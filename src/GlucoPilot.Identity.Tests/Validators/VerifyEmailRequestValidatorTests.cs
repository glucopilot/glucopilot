using FluentValidation.TestHelper;
using GlucoPilot.Identity.Models;

namespace GlucoPilot.Identity.Tests.Validators;

[TestFixture]
internal sealed class VerifyEmailRequestValidatorTests
{
    private VerifyEmailRequest.Validator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new VerifyEmailRequest.Validator();
    }

    [Test]
    public void Should_Have_Error_When_Token_Is_Empty()
    {
        var model = new VerifyEmailRequest
        {
            Token = string.Empty
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }
}