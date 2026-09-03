using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Memberships.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Memberships.Commands;

public sealed record ChangeMembershipPlanCommand(
    int MembershipId,
    ChangeMembershipPlanRequest Plan)
    : IRequest<Result<int, Error>>;