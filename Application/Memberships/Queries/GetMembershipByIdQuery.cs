using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Memberships.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Memberships.Queries;

public sealed record GetMembershipByIdQuery(
    int MembershipId)
    : IRequest<Result<MembershipDetails, Error>>;