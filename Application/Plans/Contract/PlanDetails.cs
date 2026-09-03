namespace TitanFitness.Application.Plans.Contract;

public sealed record PlanDetails(
    int PlanId,
    string PlanName,
    decimal Price,
    int DurationInMonths,
    int MaxFreezeDays,
    int MaxFreezes,
    int GuestPassQuota,
    string AccessScope,
    bool IsPublished,
    int ActiveMembershipsCount);