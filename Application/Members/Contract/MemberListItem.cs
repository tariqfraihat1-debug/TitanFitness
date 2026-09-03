namespace TitanFitness.Application.Members.Contract;

public sealed record MemberListItem(
    int MemberId,
    string MembershipNumber,
    string FullName,
    string Status,
    string Branch,
    DateTime? LastVisit);