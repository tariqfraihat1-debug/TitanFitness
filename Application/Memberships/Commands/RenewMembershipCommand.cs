using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Memberships.Commands;

public sealed record RenewMembershipCommand(
    int MembershipId)
    : IRequest<Result<int, Error>>;