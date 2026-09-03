using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Bookings.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Bookings.Commands;

public sealed record CreateBookingCommand(
    CreateBookingRequest Booking)
    : IRequest<Result<int, Error>>;