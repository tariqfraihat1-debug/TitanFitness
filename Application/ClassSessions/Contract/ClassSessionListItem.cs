namespace TitanFitness.Application.ClassSessions.Contract;

public sealed record ClassSessionListItem(
    int SessionId,
    string ClassName,
    string StudioName,
    string TrainerName,
    DateOnly SessionDate,
    TimeOnly StartTime,
    int DurationMinutes,
    int CapacityLimit,
    int BookedCount,
    int WaitlistCount,
    string Status);