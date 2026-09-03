namespace TitanFitness.Application.Members.Contract;

public sealed record CurrentMembership(
    int MembershipId,
    int PlanId,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    int FreezesUsed,
    int FreezesAllowed,
    int GuestPassesUsed,
    int GuestPassesAllowed);