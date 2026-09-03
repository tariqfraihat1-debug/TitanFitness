namespace TitanFitness.Application.Trainers.Contract;

public sealed record CreateTrainerRequest(
    string Name,
    int BranchId,
    string? Specialty,
    string? Email,
    string? Phone,
    bool IsActive);