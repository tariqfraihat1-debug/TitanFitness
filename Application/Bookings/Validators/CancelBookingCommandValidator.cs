using FluentValidation;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.Bookings.Commands;

public sealed class CancelBookingCommandValidator
    : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.BookingId).ValidId();
    }
}