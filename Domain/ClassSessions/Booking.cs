using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.ClassSessions;

public sealed class Booking : TitanFitness.Domain.Common.Entities.Entity<int>
{
    public int SessionId { get; internal set; }
    public int MemberId { get; private set; }
    public DateTime BookedOn { get; private set; }
    public BookingStatus Status { get; private set; } = null!;
    public int? WaitlistPosition { get; private set; }
    public string? TrainerNotes { get; private set; }

    private Booking()
    {
    }

    private Booking(
        int memberId,
        DateTime bookedOn,
        BookingStatus status,
        int? waitlistPosition,
        string? trainerNotes)
    {
        MemberId = memberId;
        BookedOn = bookedOn;
        Status = status;
        WaitlistPosition = waitlistPosition;
        TrainerNotes = trainerNotes;
    }

    internal static Result<Booking, Error> Create(
        int memberId,
        DateTime bookedOn,
        BookingStatus status,
        int? waitlistPosition,
        string? trainerNotes)
    {
        // Validate the initial booking state.
        if (status != BookingStatus.Booked && status != BookingStatus.Waitlisted)
            return Result.Failure<Booking, Error>(Error.InvalidOperation("A new booking must be booked or waitlisted."));

        if (status == BookingStatus.Booked && waitlistPosition.HasValue)
            return Result.Failure<Booking, Error>(Error.InvalidOperation("A booked place cannot have a waitlist position."));

        if (status == BookingStatus.Waitlisted && (!waitlistPosition.HasValue || waitlistPosition.Value <= 0))
            return Result.Failure<Booking, Error>(Error.InvalidValue(nameof(WaitlistPosition), waitlistPosition));

        if (trainerNotes?.Length > 500)
            return Result.Failure<Booking, Error>(Error.ExceedMaxLength(nameof(TrainerNotes), 500));

        Booking booking = new(
            memberId,
            bookedOn,
            status,
            waitlistPosition,
            trainerNotes?.Trim());

        return Result.Success<Booking, Error>(booking);
    }

    internal UnitResult<Error> Cancel()
    {
        // Only an active booked or waitlisted booking can be cancelled.
        if (Status == BookingStatus.Cancelled)
            return UnitResult.Failure(Error.InvalidOperation("Booking is already cancelled."));

        if (Status == BookingStatus.Attended)
            return UnitResult.Failure(Error.InvalidOperation("An attended booking cannot be cancelled."));

        if (Status == BookingStatus.NoShow)
            return UnitResult.Failure(Error.InvalidOperation("A no-show booking cannot be cancelled."));

        Status = BookingStatus.Cancelled;
        WaitlistPosition = null;

        return UnitResult.Success<Error>();
    }

    internal UnitResult<Error> PromoteFromWaitlist()
    {
        // Move a waitlisted member into a confirmed place.
        if (Status != BookingStatus.Waitlisted)
            return UnitResult.Failure(Error.InvalidOperation("Only a waitlisted booking can be promoted."));

        Status = BookingStatus.Booked;
        WaitlistPosition = null;

        return UnitResult.Success<Error>();
    }

    internal UnitResult<Error> MarkAttended()
    {
        // Attendance can only be recorded for a confirmed booking.
        if (Status != BookingStatus.Booked)
            return UnitResult.Failure(Error.InvalidOperation("Only a booked member can be marked as attended."));

        Status = BookingStatus.Attended;

        return UnitResult.Success<Error>();
    }

    internal UnitResult<Error> MarkNoShow()
    {
        // A no-show can only be recorded for a confirmed booking.
        if (Status != BookingStatus.Booked)
            return UnitResult.Failure(Error.InvalidOperation("Only a booked member can be marked as no show."));

        Status = BookingStatus.NoShow;

        return UnitResult.Success<Error>();
    }

    internal UnitResult<Error> UpdateWaitlistPosition(int position)
    {
        // Waitlist positions only apply to waitlisted bookings.
        if (Status != BookingStatus.Waitlisted)
            return UnitResult.Failure(Error.InvalidOperation("Only a waitlisted booking can have a waitlist position."));

        if (position <= 0)
            return UnitResult.Failure(Error.InvalidValue(nameof(WaitlistPosition), position));

        WaitlistPosition = position;

        return UnitResult.Success<Error>();
    }
}