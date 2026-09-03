using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Memberships;

public sealed class GuestPass : TitanFitness.Domain.Common.Entities.Entity<int>
{
    public int MembershipId { get; internal set; }
    public DateOnly IssuedOn { get; private set; }
    public DateOnly? UsedOn { get; private set; }
    public string? GuestName { get; private set; }

    private GuestPass()
    {
    }

    private GuestPass(DateOnly issuedOn, string? guestName)
    {
        IssuedOn = issuedOn;
        GuestName = guestName;
    }

    internal static Result<GuestPass, Error> Create(DateOnly issuedOn, string? guestName)
    {
        // Validate the optional guest name.
        if (guestName?.Length > 100)
            return Result.Failure<GuestPass, Error>(Error.ExceedMaxLength(nameof(GuestName), 100));

        GuestPass guestPass = new(
            issuedOn,
            guestName?.Trim());

        return Result.Success<GuestPass, Error>(guestPass);
    }

    internal UnitResult<Error> MarkAsUsed(DateOnly usedOn, string? guestName)
    {
        // A guest pass can only be used once.
        if (UsedOn.HasValue)
            return UnitResult.Failure(Error.InvalidOperation("Guest pass has already been used."));

        if (usedOn < IssuedOn)
            return UnitResult.Failure(Error.InvalidOperation("Guest pass cannot be used before it is issued."));

        if (guestName?.Length > 100)
            return UnitResult.Failure(Error.ExceedMaxLength(nameof(GuestName), 100));

        UsedOn = usedOn;
        GuestName = guestName?.Trim();

        return UnitResult.Success<Error>();
    }
}