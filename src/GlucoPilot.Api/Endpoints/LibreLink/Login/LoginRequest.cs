using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Api.Endpoints.LibreLink.Login
{
    public class LoginRequest
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Password { get; set; }

        public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
        {
            public LoginRequestValidator()
            {
                RuleFor(x => x.Username).NotEmpty().WithMessage(Resources.ValidationMessages.EmailRequired).EmailAddress().WithMessage(Resources.ValidationMessages.EmailInvalid);
                RuleFor(x => x.Password).NotEmpty().WithMessage(Resources.ValidationMessages.PasswordRequired);
            }
        }
    }
}
