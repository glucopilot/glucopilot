using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace GlucoPilot.Identity.Models;

public sealed record RegisterRequest
{
    [Required][EmailAddress] public required string Email { get; init; }

    [Required] public required string Password { get; init; }

    [Required] public required string ConfirmPassword { get; init; }

    [Required] public bool AcceptedTerms { get; init; }

    public Guid? PatientId { get; init; }

    public string? FirstName { get; init; }
    public string? LastName { get; init; }

    public sealed class Validator : AbstractValidator<RegisterRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("EMAIL_REQUIRED").EmailAddress().WithMessage("EMAIL_INVALID");
            RuleFor(x => x.Password).NotEmpty().WithMessage("PASSWORD_REQUIRED").MinimumLength(6)
                .WithMessage("PASSWORD_MIN_LENGTH");
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("CONFIRM_PASSWORD_REQUIRED").Equal(x => x.Password)
                .WithMessage("CONFIRM_PASSWORD_NOT_MATCH");
            RuleFor(x => x.AcceptedTerms).Equal(true).WithMessage("TERMS_NOT_ACCEPTED");
        }
    }
}