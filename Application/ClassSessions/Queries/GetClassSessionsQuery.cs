using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.ClassSessions.Contract;
using TitanFitness.Application.Common.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.ClassSessions.Queries;

public sealed record GetClassSessionsQuery(
    int? BranchId,
    DateOnly Date,
    int Page)
    : IRequest<Result<PagedResponse<ClassSessionListItem>, Error>>;