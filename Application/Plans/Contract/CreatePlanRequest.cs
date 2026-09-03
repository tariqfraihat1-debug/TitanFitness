namespace TitanFitness.Application.Plans.Contract;

public sealed record CreatePlanRequest(
    string PlanName,
    decimal Price,
    int DurationInMonths,
    int MaxFreezeDays,
    int MaxFreezes,
    int GuestPassQuota,
    int AccessScope,
    bool IsPublished);