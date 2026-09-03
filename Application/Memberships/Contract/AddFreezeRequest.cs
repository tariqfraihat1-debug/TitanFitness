namespace TitanFitness.Application.Memberships.Contract;

public sealed record AddFreezeRequest(
    DateOnly StartDate,
    int FreezeDurationId,
    int FreezeReasonId,
    string? Notes);