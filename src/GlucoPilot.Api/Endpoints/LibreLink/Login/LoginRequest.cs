using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Api.Endpoints.LibreLink
{
    public class LoginRequest
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Password { get; set; }
    }

    public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
