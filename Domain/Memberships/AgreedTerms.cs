using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Domain.Memberships;

public sealed class AgreedTerms : ValueObject
{
    public decimal PricePaid { get; private set; }
    public int DurationInMonths { get; private set; }
    public int MaxFreezeDays { get; private set; }
    public int MaxFreezes { get; private set; }
    public int GuestPassQuota { get; private set; }
    public AccessScope AccessScope { get; private set; } = null!;

    private AgreedTerms()
    {
    }

    private AgreedTerms(
        decimal pricePaid,
        int durationInMonths,
        int maxFreezeDays,
        int maxFreezes,
        int guestPassQuota,
        AccessScope accessScope)
    {
        PricePaid = pricePaid;
        DurationInMonths = durationInMonths;
        MaxFreezeDays = maxFreezeDays;
        MaxFreezes = maxFreezes;
        GuestPassQuota = guestPassQuota;
        AccessScope = accessScope;
    }

    public static Result<AgreedTerms, Error> Create(
        decimal pricePaid,
        int durationInMonths,
        int maxFreezeDays,
        int maxFreezes,
        int guestPassQuota,
        AccessScope accessScope)
    {
        // Validate the membership terms copied from the plan.
        if (pricePaid < 0)
            return Result.Failure<AgreedTerms, Error>(Error.InvalidValue(nameof(PricePaid), pricePaid));

        if (decimal.Round(pricePaid, 2) != pricePaid)
            return Result.Failure<AgreedTerms, Error>(Error.InvalidValue(nameof(PricePaid), pricePaid));

        if (durationInMonths <= 0)
            return Result.Failure<AgreedTerms, Error>(Error.InvalidValue(nameof(DurationInMonths), durationInMonths));

        if (maxFreezeDays < 0)
            return Result.Failure<AgreedTerms, Error>(Error.InvalidValue(nameof(MaxFreezeDays), maxFreezeDays));

        if (maxFreezes < 0)
            return Result.Failure<AgreedTerms, Error>(Error.InvalidValue(nameof(MaxFreezes), maxFreezes));

        if (guestPassQuota < 0)
            return Result.Failure<AgreedTerms, Error>(Error.InvalidValue(nameof(GuestPassQuota), guestPassQuota));

        if (accessScope is null)
            return Result.Failure<AgreedTerms, Error>(Error.ValueIsRequired(nameof(AccessScope)));

        // Store the fixed terms for this membership.
        AgreedTerms agreedTerms = new(
            pricePaid,
            durationInMonths,
            maxFreezeDays,
            maxFreezes,
            guestPassQuota,
            accessScope);

        return Result.Success<AgreedTerms, Error>(agreedTerms);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PricePaid;
        yield return DurationInMonths;
        yield return MaxFreezeDays;
        yield return MaxFreezes;
        yield return GuestPassQuota;
        yield return AccessScope;
    }
}