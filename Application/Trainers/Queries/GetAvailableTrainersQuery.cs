using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Trainers.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Trainers.Queries;

public sealed record GetAvailableTrainersQuery(int BranchId)
    : IRequest<Result<List<TrainerLookupItem>, Error>>;