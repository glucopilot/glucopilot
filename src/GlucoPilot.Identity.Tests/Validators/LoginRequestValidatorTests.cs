using FluentValidation.TestHelper;
using GlucoPilot.Identity.Models;

namespace GlucoPilot.Identity.Tests.Validators;

[TestFixture]
internal sealed class LoginRequestValidatorTests
{
    private LoginRequest.Validator _validator;
    
    [SetUp]
    public void SetUp()
    {
        _validator = new LoginRequest.Validator();
    }
    
    [Test]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var model = new LoginRequest
        {
            Email = string.Empty,
            Password = "password"
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Test]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var model = new LoginRequest
        {
            Email = "invalid-email",
            Password = "password"
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var model = new LoginRequest
        {
            Email = "user@nomail.com",
            Password = string.Empty
        };
        
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}