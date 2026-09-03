namespace TitanFitness.Application.Members.Contract;

public sealed record MemberActivity(
    string ActivityType,
    string Description,
    DateTime DateTime,
    string? Result,
    string? RefusalReason);