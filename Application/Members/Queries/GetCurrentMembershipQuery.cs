using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Members.Queries;

public sealed record GetCurrentMembershipQuery(
    int MemberId)
    : IRequest<Result<CurrentMembership, Error>>;