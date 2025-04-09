using FluentValidation.TestHelper;
using GlucoPilot.Identity.Models;

namespace GlucoPilot.Identity.Tests.Validators;

[TestFixture]
internal sealed class RegisterRequestValidatorTests
{
    private RegisterRequest.Validator _validator;
    
    [SetUp]
    public void SetUp()
    {
        _validator = new RegisterRequest.Validator();
    }
    
    [Test]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var model = new RegisterRequest
        {
            Email = string.Empty,
            Password = "password",
            ConfirmPassword = "password",
            AcceptedTerms = true,
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Test]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var model = new RegisterRequest
        {
            Email = "invalid-email",
            Password = "password",
            ConfirmPassword = "password",
            AcceptedTerms = true,
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Test]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var model = new RegisterRequest
        {
            Email = "user@nomail.com",
            Password = string.Empty,
            ConfirmPassword = "password",
            AcceptedTerms = true,
        };
        
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    [Test]
    public void Should_Have_Error_When_Password_Is_LessThanSix_Characters()
    {
        var model = new RegisterRequest
        {
            Email = "user@nomail.com",
            Password = "passw",
            ConfirmPassword = "password",
            AcceptedTerms = true,
        };
        
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    public void Should_Have_Error_When_ConfirmPassword_Is_Empty()
    {
        var model = new RegisterRequest
        {
            Email = "user@nomail.com",
            Password = "password",
            ConfirmPassword = "",
            AcceptedTerms = true,
        };
        
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }
    
    [Test]
    public void Should_Have_Error_When_ConfirmPassword_Does_Not_Match_Password()
    {
        var model = new RegisterRequest
        {
            Email = "user@nomail.com",
            Password = "password",
            ConfirmPassword = "testing",
            AcceptedTerms = true,
        };
        
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Test]
    public void Should_Have_Error_When_AcceptTerms_is_False()
    {
        var model = new RegisterRequest
        {
            Email = "user@nomail.com",
            Password = "password",
            ConfirmPassword = "password",
            AcceptedTerms = false,
        };
        
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.AcceptedTerms);
    }
}