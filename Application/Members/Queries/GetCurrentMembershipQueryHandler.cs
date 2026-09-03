using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Members.Queries;

public sealed class GetCurrentMembershipQueryHandler
    : IRequestHandler<GetCurrentMembershipQuery, Result<CurrentMembership, Error>>
{
    private readonly IReadOnlyRepository<Member, int> _memberReadRepository;
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;
    private readonly IReadOnlyRepository<Plan, int> _planReadRepository;

    public GetCurrentMembershipQueryHandler(
        IReadOnlyRepository<Member, int> memberReadRepository,
        IReadOnlyRepository<Membership, int> membershipReadRepository,
        IReadOnlyRepository<Plan, int> planReadRepository)
    {
        _memberReadRepository = memberReadRepository;
        _membershipReadRepository = membershipReadRepository;
        _planReadRepository = planReadRepository;
    }

    public async Task<Result<CurrentMembership, Error>> Handle(
        GetCurrentMembershipQuery request,
        CancellationToken cancellationToken)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        // Load the current or most recent started membership with its plan and usage counts.
        var data = await (
            from membership in _membershipReadRepository
                .GetAll()
                .Include(membership => membership.Freezes)
                .Include(membership => membership.GuestPasses)
            join plan in _planReadRepository.GetAll()
                on membership.PlanId equals plan.Id
            where membership.MemberId == request.MemberId &&
                  membership.StartDate <= today
            orderby membership.StartDate descending
            select new
            {
                Membership = membership,
                Plan = plan
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Distinguish a missing member from a member without a membership.
        if (data is null)
        {
            bool memberExists = await _memberReadRepository.AnyAsync(
                member => member.Id == request.MemberId,
                cancellationToken);

            if (!memberExists)
                return Result.Failure<CurrentMembership, Error>(Error.EntityNotFound(nameof(Member), request.MemberId));

            return Result.Failure<CurrentMembership, Error>(Error.EntityNotFound("Current membership for Member", request.MemberId));
        }

        // Calculate the live membership status.
        MembershipStatus currentStatus = data.Membership.GetStatusOn(today);

        // Build the Member Profile current-plan response.
        CurrentMembership currentMembership = new(
            data.Membership.Id,
            data.Plan.Id,
            data.Plan.PlanName,
            data.Membership.StartDate,
            data.Membership.EndDate,
            currentStatus.Name,
            data.Membership.Freezes.Count,
            data.Membership.AgreedTerms.MaxFreezes,
            data.Membership.GuestPasses.Count,
            data.Membership.AgreedTerms.GuestPassQuota);

        return Result.Success<CurrentMembership, Error>(currentMembership);
    }
}