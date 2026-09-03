using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Trainers.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Trainers.Commands;

public sealed record UpdateTrainerCommand(
    int TrainerId,
    UpdateTrainerRequest Trainer)
    : IRequest<UnitResult<Error>>;