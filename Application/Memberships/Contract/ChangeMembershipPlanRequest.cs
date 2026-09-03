namespace TitanFitness.Application.Memberships.Contract;

public sealed record ChangeMembershipPlanRequest(
    int NewPlanId,
    ChangePlanEffectiveMode EffectiveMode);