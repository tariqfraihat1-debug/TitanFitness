namespace TitanFitness.Application.CheckIns.Contract;

public sealed record CheckInDetails(
    int CheckInId,
    string Result,
    string? RefusalReason);