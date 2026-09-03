using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Bookings.Commands;

public sealed record CancelBookingCommand(int BookingId)
    : IRequest<UnitResult<Error>>;