using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.LibreLink.Login;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

[TestFixture]
internal sealed class LibreLinkLoginRequestValidatorTests
{
    private readonly LoginRequest.LoginRequestValidator _validator;

    public LibreLinkLoginRequestValidatorTests()
    {
        _validator = new LoginRequest.LoginRequestValidator();
    }

    [Test]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        var model = new LoginRequest { Username = string.Empty, Password = "password" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    public void Should_Have_Error_When_Username_Is_Invalid_Email()
    {
        var model = new LoginRequest { Username = "invalid-email", Password = "password" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var model = new LoginRequest { Username = "test@nomail.com", Password = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}