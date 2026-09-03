namespace TitanFitness.Application.Members.Contract;

public sealed record EntryEligibility(
    bool IsAdmitted,
    string? RefusalReason);