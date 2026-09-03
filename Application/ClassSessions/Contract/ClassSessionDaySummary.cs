namespace TitanFitness.Application.ClassSessions.Contract;

public sealed record ClassSessionDaySummary(
    int TotalBookings,
    decimal AverageFillRate);