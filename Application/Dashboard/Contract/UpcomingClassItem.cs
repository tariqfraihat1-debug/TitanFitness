namespace TitanFitness.Application.Dashboard.Contract;

public sealed record UpcomingClassItem(
    int SessionId,
    string ClassName,
    DateOnly SessionDate,
    TimeOnly StartTime,
    string StudioName,
    string TrainerName,
    int BookedCount,
    int CapacityLimit,
    string Status);