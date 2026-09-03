namespace TitanFitness.Application.Plans.Contract;

public sealed record PlanLookupItem(
    int PlanId,
    string PlanName,
    decimal Price,
    int DurationInMonths);