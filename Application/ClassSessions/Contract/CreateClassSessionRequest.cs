namespace TitanFitness.Application.ClassSessions.Contract;

public sealed record CreateClassSessionRequest(
    string ClassName,
    int BranchId,
    int StudioId,
    int TrainerId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    int DurationMinutes,
    int CapacityLimit,
    string? Description);