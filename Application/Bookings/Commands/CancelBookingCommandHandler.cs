using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Bookings.Commands;

public sealed class CancelBookingCommandHandler
    : IRequestHandler<CancelBookingCommand, UnitResult<Error>>
{
    private readonly IWriteRepository<ClassSession> _classSessionWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(
        IWriteRepository<ClassSession> classSessionWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _classSessionWriteRepository = classSessionWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<Error>> Handle(
        CancelBookingCommand request,
        CancellationToken cancellationToken)
    {
        // Load the class session that owns the booking and its bookings.
        Maybe<ClassSession> classSession = await _classSessionWriteRepository
            .Query()
            .Include(session => session.Bookings)
            .FirstOrDefaultAsync(
                session => session.Bookings.Any(
                    booking => booking.Id == request.BookingId),
                cancellationToken);

        if (classSession.HasNoValue)
            return UnitResult.Failure(
                Error.EntityNotFound(nameof(Booking), request.BookingId));

        // Let the aggregate cancel the booking and promote the waitlist when needed.
        UnitResult<Error> cancelResult = classSession.Value.CancelBooking(
            request.BookingId,
            DateTime.Now);

        if (cancelResult.IsFailure)
            return cancelResult;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}