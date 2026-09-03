using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Common.Contract;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Members.Queries;

public sealed record GetMembersQuery(
    int? BranchId,
    string? Search,
    int Page)
    : IRequest<Result<PagedResponse<MemberListItem>, Error>>;