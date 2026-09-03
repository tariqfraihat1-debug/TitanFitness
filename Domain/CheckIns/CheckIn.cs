using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.CheckIns;

public sealed class CheckIn : TitanFitness.Domain.Common.Entities.Entity<int>, IAggregateRoot
{
    public int MemberId { get; private set; }
    public int BranchId { get; private set; }
    public DateTime DateTime { get; private set; }
    public CheckInResult CheckInResult { get; private set; } = null!;
    public string? RefusalReason { get; private set; }

    private CheckIn()
    {
    }

    private CheckIn(
        int memberId,
        int branchId,
        DateTime dateTime,
        CheckInResult checkInResult,
        string? refusalReason)
    {
        MemberId = memberId;
        BranchId = branchId;
        DateTime = dateTime;
        CheckInResult = checkInResult;
        RefusalReason = refusalReason;
    }

    public static Result<CheckIn, Error> Create(
        int memberId,
        int branchId,
        DateTime dateTime,
        CheckInResult checkInResult,
        string? refusalReason)
    {
        // Validate the required references.
        if (memberId <= 0)
            return Result.Failure<CheckIn, Error>(Error.InvalidValue(nameof(MemberId), memberId));

        if (branchId <= 0)
            return Result.Failure<CheckIn, Error>(Error.InvalidValue(nameof(BranchId), branchId));

        // Validate the check-in result and refusal reason.
        if (checkInResult is null)
            return Result.Failure<CheckIn, Error>(Error.ValueIsRequired(nameof(CheckInResult)));

        if (checkInResult == CheckInResult.Refused && string.IsNullOrWhiteSpace(refusalReason))
            return Result.Failure<CheckIn, Error>(Error.ValueIsRequired(nameof(RefusalReason)));

        if (refusalReason?.Length > 100)
            return Result.Failure<CheckIn, Error>(Error.ExceedMaxLength(nameof(RefusalReason), 100));

        // Admitted check-ins never carry a refusal reason.
        if (checkInResult == CheckInResult.Admitted)
            refusalReason = null;

        CheckIn checkIn = new(
            memberId,
            branchId,
            dateTime,
            checkInResult,
            refusalReason?.Trim());

        return Result.Success<CheckIn, Error>(checkIn);
    }
}