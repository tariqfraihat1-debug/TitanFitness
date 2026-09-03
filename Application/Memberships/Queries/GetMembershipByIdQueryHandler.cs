using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Memberships.Contract;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Memberships.Queries;

public sealed class GetMembershipByIdQueryHandler
    : IRequestHandler<GetMembershipByIdQuery, Result<MembershipDetails, Error>>
{
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;
    private readonly IReadOnlyRepository<Plan, int> _planReadRepository;

    public GetMembershipByIdQueryHandler(
        IReadOnlyRepository<Membership, int> membershipReadRepository,
        IReadOnlyRepository<Plan, int> planReadRepository)
    {
        _membershipReadRepository = membershipReadRepository;
        _planReadRepository = planReadRepository;
    }

    public async Task<Result<MembershipDetails, Error>> Handle(
        GetMembershipByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Load the membership, plan and freezes in one query.
        var data = await (
            from membership in _membershipReadRepository
                .GetAll()
                .Include(membership => membership.Freezes)
            join plan in _planReadRepository.GetAll()
                on membership.PlanId equals plan.Id
            where membership.Id == request.MembershipId
            select new
            {
                Membership = membership,
                Plan = plan
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
            return Result.Failure<MembershipDetails, Error>(Error.EntityNotFound(nameof(Membership), request.MembershipId));

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Calculate the live membership status.
        MembershipStatus currentStatus = data.Membership.GetStatusOn(today);

        // Build the membership details response.
        MembershipDetails membershipDetails = new(
            data.Membership.Id,
            data.Membership.MemberId,
            data.Membership.PlanId,
            data.Plan.PlanName,
            data.Membership.PurchaseDate,
            data.Membership.StartDate,
            data.Membership.EndDate,
            currentStatus.Name,
            data.Membership.AgreedTerms.PricePaid,
            data.Membership.AgreedTerms.DurationInMonths,
            data.Membership.AgreedTerms.MaxFreezeDays,
            data.Membership.AgreedTerms.MaxFreezes,
            data.Membership.AgreedTerms.GuestPassQuota,
            data.Membership.AgreedTerms.AccessScope.Name);

        return Result.Success<MembershipDetails, Error>(membershipDetails);
    }
}