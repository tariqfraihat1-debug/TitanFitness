using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.ClassSessions.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.ClassSessions.Commands;

public sealed record CreateClassSessionCommand(
    CreateClassSessionRequest ClassSession)
    : IRequest<Result<int, Error>>;