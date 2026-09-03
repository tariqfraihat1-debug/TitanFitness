using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Bookings.Commands;

public sealed class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, Result<int, Error>>
{
    private readonly IReadOnlyRepository<Member, int> _memberReadRepository;
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;
    private readonly IWriteRepository<ClassSession> _classSessionWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(
        IReadOnlyRepository<Member, int> memberReadRepository,
        IReadOnlyRepository<Membership, int> membershipReadRepository,
        IWriteRepository<ClassSession> classSessionWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _memberReadRepository = memberReadRepository;
        _membershipReadRepository = membershipReadRepository;
        _classSessionWriteRepository = classSessionWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int, Error>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        DateTime now = DateTime.Now;

        // Load the session and its bookings for domain validation.
        Maybe<ClassSession> classSession = await _classSessionWriteRepository
            .Query()
            .Include(session => session.Bookings)
            .FirstOrDefaultAsync(
                session => session.Id == request.Booking.SessionId,
                cancellationToken);

        if (classSession.HasNoValue)
            return Result.Failure<int, Error>(Error.EntityNotFound(nameof(ClassSession), request.Booking.SessionId));

        DateOnly sessionDate = classSession.Value.SessionDate;

        // Load the membership covering the session date with its freezes.
        Maybe<Membership> membership = await _membershipReadRepository
            .GetAll()
            .Include(membership => membership.Freezes)
            .Where(membership =>
                membership.MemberId == request.Booking.MemberId &&
                membership.StartDate <= sessionDate &&
                membership.EndDate >= sessionDate)
            .OrderByDescending(membership => membership.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        // Distinguish a missing member from a member without an active membership.
        if (membership.HasNoValue)
        {
            bool memberExists = await _memberReadRepository.AnyAsync(
                member => member.Id == request.Booking.MemberId,
                cancellationToken);

            if (!memberExists)
                return Result.Failure<int, Error>(Error.EntityNotFound(nameof(Member), request.Booking.MemberId));

            return Result.Failure<int, Error>(Error.InvalidOperation("Member must have an active membership to book a class."));
        }

        // Ensure the membership is active on the class date.
        if (!membership.Value.IsActiveOn(sessionDate))
            return Result.Failure<int, Error>(Error.InvalidOperation("Member must have an active membership to book a class."));

        // Load the member's other bookings that could overlap on the same day.
        var existingSessions = await _classSessionWriteRepository
            .Query()
            .Where(session =>
                session.Id != classSession.Value.Id &&
                session.SessionDate == sessionDate &&
                session.Bookings.Any(booking =>
                    booking.MemberId == request.Booking.MemberId &&
                    booking.Status != BookingStatus.Cancelled))
            .Select(session => new
            {
                session.StartTime,
                session.DurationMinutes
            })
            .ToListAsync(cancellationToken);

        DateTime newSessionStart = sessionDate.ToDateTime(classSession.Value.StartTime);
        DateTime newSessionEnd = newSessionStart.AddMinutes(classSession.Value.DurationMinutes);

        // Prevent the member from booking overlapping class sessions.
        bool overlapsExistingBooking = existingSessions.Any(session =>
        {
            DateTime existingSessionStart = sessionDate.ToDateTime(session.StartTime);
            DateTime existingSessionEnd = existingSessionStart.AddMinutes(session.DurationMinutes);

            return existingSessionStart < newSessionEnd &&
                   existingSessionEnd > newSessionStart;
        });

        if (overlapsExistingBooking)
            return Result.Failure<int, Error>(Error.InvalidOperation("Member cannot be booked into overlapping class sessions."));

        // Let the aggregate validate and add the booking.
        Result<Booking, Error> bookingResult = classSession.Value.AddBooking(
            request.Booking.MemberId,
            now,
            request.Booking.TrainerNotes,
            now);

        if (bookingResult.IsFailure)
            return Result.Failure<int, Error>(bookingResult.Error);

        // Persist the new booking through the tracked class session.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<int, Error>(bookingResult.Value.Id);
    }
}