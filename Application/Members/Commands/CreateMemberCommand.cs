using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Members.Commands;

public sealed record CreateMemberCommand(
    CreateMemberRequest Member)
    : IRequest<Result<int, Error>>;