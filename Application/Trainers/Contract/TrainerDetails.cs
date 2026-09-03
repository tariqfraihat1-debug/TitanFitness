namespace TitanFitness.Application.Trainers.Contract;

public sealed record TrainerDetails(
    int TrainerId,
    string Name,
    int BranchId,
    string BranchName,
    string? Specialty,
    string? Email,
    string? Phone,
    bool IsActive);