using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Plans.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Memberships.Queries;

public sealed record GetAvailablePlansQuery(
    int MembershipId)
    : IRequest<Result<List<PlanLookupItem>, Error>>;