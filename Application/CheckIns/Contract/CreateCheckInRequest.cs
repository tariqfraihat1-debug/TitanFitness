namespace TitanFitness.Application.CheckIns.Contract;

public sealed record CreateCheckInRequest(
    int MemberId,
    int BranchId);