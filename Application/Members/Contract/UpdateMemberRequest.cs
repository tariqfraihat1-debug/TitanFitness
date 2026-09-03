namespace TitanFitness.Application.Members.Contract;

public sealed record UpdateMemberRequest(
    string FullName,
    string? Email,
    string? Phone,
    string? Address,
    DateOnly JoinedDate,
    string? Photo,
    int HomeBranchId);