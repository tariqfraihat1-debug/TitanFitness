namespace TitanFitness.Application.Dashboard.Contract;

public sealed record CheckInsTodayDetails(
    int CheckInsToday,
    decimal PercentageChange);