using FluentValidation;
using GlucoPilot.Api.Models;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using System;

namespace GlucoPilot.Api.Endpoints.Pens.NewPen
{
    public record NewPenRequest
    {
        public Guid InsulinId { get; set; }
        public PenModel Model { get; set; }
        public PenColour Colour { get; set; }
        public string Serial { get; set; } = string.Empty;
        public DateTimeOffset? StartTime { get; set; }

        public sealed class NewPenRequestValidator : AbstractValidator<NewPenRequest>
        {
            public NewPenRequestValidator(ICurrentUser currentUser, IRepository<Insulin> insulinRepository)
            {
                RuleFor(x => x.Model)
                    .IsInEnum().WithMessage(Resources.ValidationMessages.PenModelInvalid);

                RuleFor(x => x.Colour)
                    .IsInEnum().WithMessage(Resources.ValidationMessages.PenColourInvalid);

                RuleFor(x => x.Serial)
                    .NotEmpty().WithMessage(Resources.ValidationMessages.PenSerialRequired)
                    .Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage(Resources.ValidationMessages.PenSerialInvalid)
                    .MaximumLength(100).WithMessage(Resources.ValidationMessages.PenSerialTooLong);

                RuleFor(x => x.InsulinId)
                    .MustAsync(async (id, cancellation) => {
                        return await insulinRepository.FindOneAsync(i => i.Id == id && i.UserId == currentUser.GetUserId(), new FindOptions { IsAsNoTracking = true }, cancellation).ConfigureAwait(false) is not null;
                    })
                    .WithMessage(Resources.ValidationMessages.InsulinNotFound);
            }
        }
    }
}