using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace GlucoPilot.Identity.Models;

public sealed record RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
    
    [Required]
    public required string Password { get; init; }
    
    [Required]
    public required string ConfirmPassword { get; init; }
    
    [Required]
    public bool AcceptedTerms { get; init; }
    
    public Guid? PatientId { get; init; }
    
    public sealed class Validator : AbstractValidator<RegisterRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.Password);
            RuleFor(x => x.AcceptedTerms).Equal(true);
        }
    }
}