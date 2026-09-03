using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Dashboard.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Dashboard.Queries;

public sealed record GetActiveMembersQuery()
    : IRequest<Result<ActiveMembersDetails, Error>>;