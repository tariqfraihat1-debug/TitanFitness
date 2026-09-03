using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Members.Queries;

public sealed record GetMemberByIdQuery(
    int MemberId)
    : IRequest<Result<MemberDetails, Error>>;