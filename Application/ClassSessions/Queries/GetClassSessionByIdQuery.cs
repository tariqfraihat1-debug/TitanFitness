using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.ClassSessions.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.ClassSessions.Queries;

public sealed record GetClassSessionByIdQuery(
    int SessionId)
    : IRequest<Result<ClassSessionDetails, Error>>;