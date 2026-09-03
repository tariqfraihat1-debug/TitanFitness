namespace TitanFitness.Application.Memberships.Contract;

public sealed record MembershipDetails(
    int MembershipId,
    int MemberId,
    int PlanId,
    string PlanName,
    DateTime PurchaseDate,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    decimal PricePaid,
    int DurationInMonths,
    int MaximumFreezeDays,
    int MaximumNumberOfFreezes,
    int GuestPassQuota,
    string AccessScope);