namespace TitanFitness.Application.Studios.Contract;

public sealed record StudioResponse(
    int StudioId,
    string Name,
    int Capacity);