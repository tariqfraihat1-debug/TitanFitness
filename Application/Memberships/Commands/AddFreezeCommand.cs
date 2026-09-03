using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Memberships.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Memberships.Commands;

public sealed record AddFreezeCommand(
    int MembershipId,
    AddFreezeRequest Freeze)
    : IRequest<Result<int, Error>>;