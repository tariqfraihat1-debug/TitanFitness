namespace TitanFitness.Application.Trainers.Contract;

public sealed record TrainerLookupItem(
    int TrainerId,
    string Name,
    string? Specialty);