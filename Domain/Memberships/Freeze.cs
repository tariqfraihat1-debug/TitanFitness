using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Memberships;

public sealed class Freeze : TitanFitness.Domain.Common.Entities.Entity<int>
{
    public int MembershipId { get; internal set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public int DurationInMonths { get; private set; }
    public FreezeReason Reason { get; private set; } = null!;
    public string? AdditionalNotes { get; private set; }
    public DateTime RequestedOn { get; private set; }

    private Freeze()
    {
    }

    private Freeze(
        DateOnly startDate,
        DateOnly endDate,
        int durationInMonths,
        FreezeReason reason,
        string? additionalNotes,
        DateTime requestedOn)
    {
        StartDate = startDate;
        EndDate = endDate;
        DurationInMonths = durationInMonths;
        Reason = reason;
        AdditionalNotes = additionalNotes;
        RequestedOn = requestedOn;
    }

    internal static Result<Freeze, Error> Create(
        DateOnly startDate,
        int durationInMonths,
        FreezeReason reason,
        string? additionalNotes,
        DateTime requestedOn)
    {
        // Validate the freeze details.
        if (durationInMonths <= 0)
            return Result.Failure<Freeze, Error>(Error.InvalidValue(nameof(DurationInMonths), durationInMonths));

        if (reason is null)
            return Result.Failure<Freeze, Error>(Error.ValueIsRequired(nameof(Reason)));

        if (additionalNotes?.Length > 200)
            return Result.Failure<Freeze, Error>(Error.ExceedMaxLength(nameof(AdditionalNotes), 200));

        // Calculate the freeze period from the selected duration.
        DateOnly endDate = startDate.AddMonths(durationInMonths).AddDays(-1);

        Freeze freeze = new(
            startDate,
            endDate,
            durationInMonths,
            reason,
            additionalNotes?.Trim(),
            requestedOn);

        return Result.Success<Freeze, Error>(freeze);
    }
}