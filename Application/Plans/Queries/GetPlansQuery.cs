using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Common.Contract;
using TitanFitness.Application.Plans.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Plans.Queries;

public sealed record GetPlansQuery(
    int? AccessScope,
    string? Search,
    int Page)
    : IRequest<Result<PagedResponse<PlanListItem>, Error>>;