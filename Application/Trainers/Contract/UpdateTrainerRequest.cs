namespace TitanFitness.Application.Trainers.Contract;

public sealed record UpdateTrainerRequest(
    string Name,
    int BranchId,
    string? Specialty,
    string? Email,
    string? Phone,
    bool IsActive);