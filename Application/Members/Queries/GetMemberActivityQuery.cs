using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Members.Queries;

public sealed record GetMemberActivityQuery(
    int MemberId)
    : IRequest<Result<List<MemberActivity>, Error>>;