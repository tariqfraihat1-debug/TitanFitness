using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.ClassSessions;

public sealed class ClassSession : TitanFitness.Domain.Common.Entities.Entity<int>, IAggregateRoot
{
    private readonly List<Booking> _bookings = [];

    public string ClassName { get; private set; } = null!;
    public int BranchId { get; private set; }
    public int StudioId { get; private set; }
    public int TrainerId { get; private set; }
    public DateOnly SessionDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public int CapacityLimit { get; private set; }
    public ClassSessionStatus Status { get; private set; } = null!;
    public string? Description { get; private set; }
    public IReadOnlyList<Booking> Bookings => _bookings;

    private ClassSession()
    {
    }

    private ClassSession(
        string className,
        int branchId,
        int studioId,
        int trainerId,
        DateOnly sessionDate,
        TimeOnly startTime,
        int durationMinutes,
        int capacityLimit,
        string? description)
    {
        ClassName = className;
        BranchId = branchId;
        StudioId = studioId;
        TrainerId = trainerId;
        SessionDate = sessionDate;
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        CapacityLimit = capacityLimit;
        Status = ClassSessionStatus.Open;
        Description = description;
    }

    // Validate and create a new class session.
    public static Result<ClassSession, Error> Create(
        string className,
        int branchId,
        int studioId,
        int trainerId,
        DateOnly sessionDate,
        TimeOnly startTime,
        int durationMinutes,
        int capacityLimit,
        string? description,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(className))
            return Result.Failure<ClassSession, Error>(Error.ValueIsRequired(nameof(ClassName)));

        if (className.Length > 100)
            return Result.Failure<ClassSession, Error>(Error.ExceedMaxLength(nameof(ClassName), 100));

        if (durationMinutes != 30 && durationMinutes != 45 && durationMinutes != 60)
            return Result.Failure<ClassSession, Error>(Error.InvalidValue(nameof(DurationMinutes), durationMinutes));

        if (capacityLimit <= 0)
            return Result.Failure<ClassSession, Error>(Error.InvalidValue(nameof(CapacityLimit), capacityLimit));

        if (description?.Length > 500)
            return Result.Failure<ClassSession, Error>(Error.ExceedMaxLength(nameof(Description), 500));

        ClassSession session = new(
            className.Trim(),
            branchId,
            studioId,
            trainerId,
            sessionDate,
            startTime,
            durationMinutes,
            capacityLimit,
            description?.Trim());

        session.RefreshStatus(now);

        return Result.Success<ClassSession, Error>(session);
    }

    // Add a confirmed booking or place the member on the waitlist when full.
    public Result<Booking, Error> AddBooking(
        int memberId,
        DateTime bookedOn,
        string? trainerNotes,
        DateTime now)
    {
        RefreshStatus(now);

        if (Status != ClassSessionStatus.Open)
            return Result.Failure<Booking, Error>(Error.InvalidOperation("Bookings can only be added to an open session."));

        if (_bookings.Any(booking => booking.MemberId == memberId && booking.Status != BookingStatus.Cancelled))
            return Result.Failure<Booking, Error>(Error.InvalidOperation("Member already has a booking for this session."));

        int bookedCount = _bookings.Count(booking => booking.Status == BookingStatus.Booked);

        BookingStatus bookingStatus;
        int? waitlistPosition = null;

        if (bookedCount < CapacityLimit)
        {
            bookingStatus = BookingStatus.Booked;
        }
        else
        {
            bookingStatus = BookingStatus.Waitlisted;
            waitlistPosition = _bookings.Count(booking => booking.Status == BookingStatus.Waitlisted) + 1;
        }

        Result<Booking, Error> bookingResult = Booking.Create(
            memberId,
            bookedOn,
            bookingStatus,
            waitlistPosition,
            trainerNotes);

        if (bookingResult.IsFailure)
            return bookingResult;

        _bookings.Add(bookingResult.Value);

        return Result.Success<Booking, Error>(bookingResult.Value);
    }

    // Cancel a booking and promote the first waiting member when a place opens.
    public UnitResult<Error> CancelBooking(int bookingId, DateTime now)
    {
        RefreshStatus(now);

        Maybe<Booking> booking = _bookings.FirstOrDefault(booking => booking.Id == bookingId);

        if (booking.HasNoValue)
            return UnitResult.Failure(Error.EntityNotFound(nameof(Booking), bookingId));

        bool wasBooked = booking.Value.Status == BookingStatus.Booked;

        UnitResult<Error> cancelResult = booking.Value.Cancel();

        if (cancelResult.IsFailure)
            return cancelResult;

        if (wasBooked && Status == ClassSessionStatus.Open)
        {
            Maybe<Booking> firstWaitlisted = _bookings
                .Where(booking => booking.Status == BookingStatus.Waitlisted)
                .OrderBy(booking => booking.WaitlistPosition)
                .FirstOrDefault();

            if (firstWaitlisted.HasValue)
            {
                UnitResult<Error> promoteResult = firstWaitlisted.Value.PromoteFromWaitlist();

                if (promoteResult.IsFailure)
                    return promoteResult;
            }
        }

        ReorderWaitlist();

        return UnitResult.Success<Error>();
    }

    // Record attendance for a confirmed booking.
    public UnitResult<Error> MarkAttended(int bookingId)
    {
        Maybe<Booking> booking = _bookings.FirstOrDefault(booking => booking.Id == bookingId);

        if (booking.HasNoValue)
            return UnitResult.Failure(Error.EntityNotFound(nameof(Booking), bookingId));

        return booking.Value.MarkAttended();
    }

    // Record a no-show for a confirmed booking.
    public UnitResult<Error> MarkNoShow(int bookingId)
    {
        Maybe<Booking> booking = _bookings.FirstOrDefault(booking => booking.Id == bookingId);

        if (booking.HasNoValue)
            return UnitResult.Failure(Error.EntityNotFound(nameof(Booking), bookingId));

        return booking.Value.MarkNoShow();
    }

    // Cancel a session that has not completed.
    public UnitResult<Error> Cancel(DateTime now)
    {
        RefreshStatus(now);

        if (Status == ClassSessionStatus.Cancelled)
            return UnitResult.Failure(Error.InvalidOperation("Session is already cancelled."));

        if (Status == ClassSessionStatus.Completed)
            return UnitResult.Failure(Error.InvalidOperation("A completed session cannot be cancelled."));

        Status = ClassSessionStatus.Cancelled;

        return UnitResult.Success<Error>();
    }

    // Calculate the live session status without changing the entity.
    public ClassSessionStatus GetStatusAt(DateTime dateTime)
    {
        if (Status == ClassSessionStatus.Cancelled)
            return ClassSessionStatus.Cancelled;

        DateTime sessionStart = SessionDate.ToDateTime(StartTime);
        DateTime sessionEnd = sessionStart.AddMinutes(DurationMinutes);

        if (dateTime < sessionStart)
            return ClassSessionStatus.Open;

        if (dateTime < sessionEnd)
            return ClassSessionStatus.InProgress;

        return ClassSessionStatus.Completed;
    }

    // Refresh the persisted status for write-side operations.
    public void RefreshStatus(DateTime now)
    {
        if (Status == ClassSessionStatus.Cancelled)
            return;

        Status = GetStatusAt(now);
    }

    // Keep waiting-list positions sequential.
    private void ReorderWaitlist()
    {
        List<Booking> waitlistedBookings = _bookings
            .Where(booking => booking.Status == BookingStatus.Waitlisted)
            .OrderBy(booking => booking.WaitlistPosition)
            .ToList();

        for (int i = 0; i < waitlistedBookings.Count; i++)
            waitlistedBookings[i].UpdateWaitlistPosition(i + 1);
    }
}