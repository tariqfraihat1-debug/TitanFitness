namespace TitanFitness.Application.ClassSessions.Contract;

public sealed record ClassSessionDetails(
    int SessionId,
    string ClassName,
    int BranchId,
    int StudioId,
    string StudioName,
    int TrainerId,
    string TrainerName,
    DateOnly SessionDate,
    TimeOnly StartTime,
    int DurationMinutes,
    int CapacityLimit,
    int BookedCount,
    int WaitlistCount,
    int RemainingPlaces,
    string Status,
    string? Description);