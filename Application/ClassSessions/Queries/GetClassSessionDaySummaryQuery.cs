using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.ClassSessions.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.ClassSessions.Queries;

public sealed record GetClassSessionDaySummaryQuery(
    int? BranchId,
    DateOnly Date)
    : IRequest<Result<ClassSessionDaySummary, Error>>;