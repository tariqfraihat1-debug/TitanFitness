using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Memberships;

public sealed class Membership : TitanFitness.Domain.Common.Entities.Entity<int>, IAggregateRoot
{
    private readonly List<Freeze> _freezes = [];
    private readonly List<GuestPass> _guestPasses = [];

    public int MemberId { get; private set; }
    public int PlanId { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public MembershipStatus Status { get; private set; } = null!;
    public AgreedTerms AgreedTerms { get; private set; } = null!;
    public IReadOnlyList<Freeze> Freezes => _freezes;
    public IReadOnlyList<GuestPass> GuestPasses => _guestPasses;

    private Membership()
    {
    }

    private Membership(
        int memberId,
        int planId,
        DateTime purchaseDate,
        DateOnly startDate,
        DateOnly endDate,
        AgreedTerms agreedTerms)
    {
        MemberId = memberId;
        PlanId = planId;
        PurchaseDate = purchaseDate;
        StartDate = startDate;
        EndDate = endDate;
        AgreedTerms = agreedTerms;
    }

    public static Result<Membership, Error> Create(
        int memberId,
        int planId,
        DateTime purchaseDate,
        DateOnly startDate,
        AgreedTerms agreedTerms,
        DateOnly today)
    {
        // Validate the fixed membership terms.
        if (agreedTerms is null)
            return Result.Failure<Membership, Error>(Error.ValueIsRequired(nameof(AgreedTerms)));

        // Calculate the membership period from the agreed duration.
        DateOnly endDate = startDate
            .AddMonths(agreedTerms.DurationInMonths)
            .AddDays(-1);

        Membership membership = new(
            memberId,
            planId,
            purchaseDate,
            startDate,
            endDate,
            agreedTerms);

        membership.RefreshStatus(today);

        return Result.Success<Membership, Error>(membership);
    }

    public Result<Freeze, Error> AddFreeze(
        DateOnly startDate,
        int durationInMonths,
        FreezeReason reason,
        string? notes,
        DateTime requestedOn,
        DateOnly today)
    {
        // A freeze can only be added to an active membership.
        RefreshStatus(today);

        if (Status != MembershipStatus.Active)
            return Result.Failure<Freeze, Error>(Error.InvalidOperation("Only an active membership can be frozen."));

        if (startDate < today)
            return Result.Failure<Freeze, Error>(Error.InvalidOperation("A freeze cannot begin in the past."));

        Result<Freeze, Error> freezeResult = Freeze.Create(
            startDate,
            durationInMonths,
            reason,
            notes,
            requestedOn);

        if (freezeResult.IsFailure)
            return freezeResult;

        Freeze freeze = freezeResult.Value;

        // Enforce the membership freeze rules and agreed allowance.
        if (_freezes.Any(existingFreeze => existingFreeze.StartDate <= freeze.EndDate && existingFreeze.EndDate >= freeze.StartDate))
            return Result.Failure<Freeze, Error>(Error.InvalidOperation("Freeze periods cannot overlap."));

        if (freeze.EndDate > EndDate)
            return Result.Failure<Freeze, Error>(Error.InvalidOperation("A freeze cannot run past the membership end date."));

        if (_freezes.Count >= AgreedTerms.MaxFreezes)
            return Result.Failure<Freeze, Error>(Error.InvalidOperation("Maximum number of freezes has been reached."));

        int usedFreezeDays = _freezes.Sum(existingFreeze =>
            existingFreeze.EndDate.DayNumber - existingFreeze.StartDate.DayNumber + 1);

        int newFreezeDays =
            freeze.EndDate.DayNumber - freeze.StartDate.DayNumber + 1;

        if (usedFreezeDays + newFreezeDays > AgreedTerms.MaxFreezeDays)
            return Result.Failure<Freeze, Error>(Error.InvalidOperation("Maximum freeze days would be exceeded."));

        _freezes.Add(freeze);

        // Extend the membership by exactly the frozen days.
        EndDate = EndDate.AddDays(newFreezeDays);

        RefreshStatus(today);

        return Result.Success<Freeze, Error>(freeze);
    }

    public Result<GuestPass, Error> IssueGuestPass(
        DateOnly issuedOn,
        string? guestName,
        DateOnly today)
    {
        // Guest passes can only come from an active membership.
        RefreshStatus(today);

        if (Status != MembershipStatus.Active)
            return Result.Failure<GuestPass, Error>(Error.InvalidOperation("Guest passes can only be issued from an active membership."));

        if (_guestPasses.Count >= AgreedTerms.GuestPassQuota)
            return Result.Failure<GuestPass, Error>(Error.InvalidOperation("Guest pass quota has been reached."));

        Result<GuestPass, Error> guestPassResult = GuestPass.Create(
            issuedOn,
            guestName);

        if (guestPassResult.IsFailure)
            return guestPassResult;

        _guestPasses.Add(guestPassResult.Value);

        return Result.Success<GuestPass, Error>(guestPassResult.Value);
    }

    public UnitResult<Error> UseGuestPass(
        int guestPassId,
        DateOnly usedOn,
        string? guestName,
        DateOnly today)
    {
        // Guest passes can only be used while the membership is active.
        RefreshStatus(today);

        if (Status != MembershipStatus.Active)
            return UnitResult.Failure(Error.InvalidOperation("Guest passes can only be used with an active membership."));

        Maybe<GuestPass> guestPass = _guestPasses.FirstOrDefault(guestPass => guestPass.Id == guestPassId);

        if (guestPass.HasNoValue)
            return UnitResult.Failure(Error.EntityNotFound(nameof(GuestPass), guestPassId));

        UnitResult<Error> useResult = guestPass.Value.MarkAsUsed(
            usedOn,
            guestName);

        if (useResult.IsFailure)
            return useResult;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Cancel()
    {
        // Cancellation is final.
        if (Status == MembershipStatus.Cancelled)
            return UnitResult.Failure(Error.InvalidOperation("Membership is already cancelled."));

        Status = MembershipStatus.Cancelled;

        return UnitResult.Success<Error>();
    }

    public bool IsFrozenOn(DateOnly date) =>
        _freezes.Any(freeze =>
            freeze.StartDate <= date &&
            freeze.EndDate >= date);

    public bool IsActiveOn(DateOnly date)
    {
        if (Status == MembershipStatus.Cancelled)
            return false;

        if (date < StartDate || date > EndDate)
            return false;

        if (IsFrozenOn(date))
            return false;

        return true;
    }

    public MembershipStatus GetStatusOn(DateOnly date)
    {
        if (Status == MembershipStatus.Cancelled)
            return MembershipStatus.Cancelled;

        if (date < StartDate)
            return MembershipStatus.Pending;

        if (date > EndDate)
            return MembershipStatus.Expired;

        return IsFrozenOn(date)
            ? MembershipStatus.Frozen
            : MembershipStatus.Active;
    }

    public void RefreshStatus(DateOnly today)
    {
        if (Status == MembershipStatus.Cancelled)
            return;

        Status = GetStatusOn(today);
    }
}