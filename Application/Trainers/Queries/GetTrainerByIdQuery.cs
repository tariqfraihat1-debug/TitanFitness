using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Trainers.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Trainers.Queries;

public sealed record GetTrainerByIdQuery(
    int TrainerId)
    : IRequest<Result<TrainerDetails, Error>>;