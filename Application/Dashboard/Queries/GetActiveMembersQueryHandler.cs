using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Dashboard.Contract;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Memberships;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Dashboard.Queries;

public sealed class GetActiveMembersQueryHandler
    : IRequestHandler<GetActiveMembersQuery, Result<ActiveMembersDetails, Error>>
{
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;

    public GetActiveMembersQueryHandler(
        IReadOnlyRepository<Membership, int> membershipReadRepository)
    {
        _membershipReadRepository = membershipReadRepository;
    }

    public async Task<Result<ActiveMembersDetails, Error>> Handle(
        GetActiveMembersQuery request,
        CancellationToken cancellationToken)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Count memberships that are active today and not currently frozen.
        int activeMembers = await _membershipReadRepository
            .GetAll()
            .CountAsync(
                membership =>
                    membership.StartDate <= today &&
                    membership.EndDate >= today &&
                    membership.Status != MembershipStatus.Cancelled &&
                    !membership.Freezes.Any(
                        freeze =>
                            freeze.StartDate <= today &&
                            freeze.EndDate >= today),
                cancellationToken);

        ActiveMembersDetails response = new(activeMembers);

        return Result.Success<ActiveMembersDetails, Error>(response);
    }
}