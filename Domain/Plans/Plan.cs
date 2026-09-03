using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Plans;

public sealed class Plan : TitanFitness.Domain.Common.Entities.Entity<int>, IAggregateRoot
{
    public string PlanName { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int DurationInMonths { get; private set; }
    public int MaxFreezeDays { get; private set; }
    public int MaxFreezes { get; private set; }
    public int GuestPassQuota { get; private set; }
    public AccessScope AccessScope { get; private set; } = null!;
    public bool IsPublished { get; private set; }

    private Plan()
    {
    }

    private Plan(
        string planName,
        decimal price,
        int durationInMonths,
        int maxFreezeDays,
        int maxFreezes,
        int guestPassQuota,
        AccessScope accessScope,
        bool isPublished)
    {
        PlanName = planName;
        Price = price;
        DurationInMonths = durationInMonths;
        MaxFreezeDays = maxFreezeDays;
        MaxFreezes = maxFreezes;
        GuestPassQuota = guestPassQuota;
        AccessScope = accessScope;
        IsPublished = isPublished;
    }

    // Validate and create a new membership plan.
    public static Result<Plan, Error> Create(
        string planName,
        decimal price,
        int durationInMonths,
        int maxFreezeDays,
        int maxFreezes,
        int guestPassQuota,
        AccessScope accessScope,
        bool isPublished)
    {
        UnitResult<Error> validationResult = Validate(
            planName,
            price,
            durationInMonths,
            maxFreezeDays,
            maxFreezes,
            guestPassQuota,
            accessScope);

        if (validationResult.IsFailure)
            return Result.Failure<Plan, Error>(validationResult.Error);

        Plan plan = new(
            planName.Trim(),
            price,
            durationInMonths,
            maxFreezeDays,
            maxFreezes,
            guestPassQuota,
            accessScope,
            isPublished);

        return Result.Success<Plan, Error>(plan);
    }

    // Validate and update the membership plan.
    public UnitResult<Error> Update(
        string planName,
        decimal price,
        int durationInMonths,
        int maxFreezeDays,
        int maxFreezes,
        int guestPassQuota,
        AccessScope accessScope,
        bool isPublished)
    {
        UnitResult<Error> validationResult = Validate(
            planName,
            price,
            durationInMonths,
            maxFreezeDays,
            maxFreezes,
            guestPassQuota,
            accessScope);

        if (validationResult.IsFailure)
            return UnitResult.Failure(validationResult.Error);

        PlanName = planName.Trim();
        Price = price;
        DurationInMonths = durationInMonths;
        MaxFreezeDays = maxFreezeDays;
        MaxFreezes = maxFreezes;
        GuestPassQuota = guestPassQuota;
        AccessScope = accessScope;
        IsPublished = isPublished;

        return UnitResult.Success<Error>();
    }

    // Apply the business rules shared by creation and update.
    private static UnitResult<Error> Validate(
        string planName,
        decimal price,
        int durationInMonths,
        int maxFreezeDays,
        int maxFreezes,
        int guestPassQuota,
        AccessScope accessScope)
    {
        if (string.IsNullOrWhiteSpace(planName))
            return UnitResult.Failure(Error.ValueIsRequired(nameof(PlanName)));

        if (planName.Length > 50)
            return UnitResult.Failure(Error.ExceedMaxLength(nameof(PlanName), 50));

        if (price < 0)
            return UnitResult.Failure(Error.InvalidValue(nameof(Price), price));

        if (decimal.Round(price, 2) != price)
            return UnitResult.Failure(Error.InvalidValue(nameof(Price), price));

        if (durationInMonths <= 0)
            return UnitResult.Failure(Error.InvalidValue(nameof(DurationInMonths), durationInMonths));

        if (maxFreezeDays < 0)
            return UnitResult.Failure(Error.InvalidValue(nameof(MaxFreezeDays), maxFreezeDays));

        if (maxFreezes < 0)
            return UnitResult.Failure(Error.InvalidValue(nameof(MaxFreezes), maxFreezes));

        if (guestPassQuota < 0)
            return UnitResult.Failure(Error.InvalidValue(nameof(GuestPassQuota), guestPassQuota));

        if (accessScope is null)
            return UnitResult.Failure(Error.ValueIsRequired(nameof(AccessScope)));

        return UnitResult.Success<Error>();
    }
}