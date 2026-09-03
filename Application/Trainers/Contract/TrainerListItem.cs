namespace TitanFitness.Application.Trainers.Contract;

public sealed record TrainerListItem(
    int TrainerId,
    string Name,
    string? Specialty,
    string BranchName,
    bool IsActive);