using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Common;
using TitanFitness.Application.Common.Contract;
using TitanFitness.Application.Trainers.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Trainers.Queries;

public sealed record GetTrainersQuery(
    int? BranchId,
    string? Search,
    int Page)
    : IRequest<Result<PagedResponse<TrainerListItem>, Error>>;