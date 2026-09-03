namespace TitanFitness.Application.Members.Contract;

public sealed record MemberDetails(
    int MemberId,
    string MembershipNumber,
    string FullName,
    string? Email,
    string? Phone,
    string? Address,
    DateOnly JoinedDate,
    string? Photo,
    int HomeBranchId);