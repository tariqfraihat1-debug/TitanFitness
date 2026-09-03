namespace TitanFitness.Application.Memberships.Contract;

public sealed record CreateMembershipRequest(
    int MemberId,
    int PlanId,
    DateOnly StartDate);