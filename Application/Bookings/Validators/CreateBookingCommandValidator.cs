using FluentValidation;
using TitanFitness.Application.Bookings.Commands;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.Bookings.Validators;

public sealed class CreateBookingCommandValidator
    : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(command => command.Booking.SessionId)
            .ValidId();

        RuleFor(command => command.Booking.MemberId)
            .ValidId();

        RuleFor(command => command.Booking.TrainerNotes)
            .MaximumLength(500)
            .When(command => !string.IsNullOrWhiteSpace(command.Booking.TrainerNotes));
    }
}