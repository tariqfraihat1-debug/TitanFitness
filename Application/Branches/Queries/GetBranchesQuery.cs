using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Branches.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Branches.Queries;

public sealed record GetBranchesQuery()
    : IRequest<Result<IReadOnlyList<BranchListItem>, Error>>;