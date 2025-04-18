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
            RuleFor(x => x.Email).NotEmpty().WithMessage(Resources.ValidationMessages.EmailRequired).EmailAddress().WithMessage(Resources.ValidationMessages.EmailInvalid);
            RuleFor(x => x.Password).NotEmpty().WithMessage(Resources.ValidationMessages.PasswordRequired).MinimumLength(6)
                .WithMessage(Resources.ValidationMessages.PasswordMinLength);
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(Resources.ValidationMessages.ConfirmPasswordRequired).Equal(x => x.Password)
                .WithMessage(Resources.ValidationMessages.ConfirmPasswordNotMatch);
            RuleFor(x => x.AcceptedTerms).Equal(true).WithMessage(Resources.ValidationMessages.TermsNotAccepted);
        }
    }
}