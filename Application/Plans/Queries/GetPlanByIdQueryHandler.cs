using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Plans.Contract;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Plans.Queries;

public sealed class GetPlanByIdQueryHandler : IRequestHandler<GetPlanByIdQuery, Result<PlanDetails, Error>>
{
    private readonly IReadOnlyRepository<Plan, int> _planReadRepository;
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;

    public GetPlanByIdQueryHandler(
        IReadOnlyRepository<Plan, int> planReadRepository,
        IReadOnlyRepository<Membership, int> membershipReadRepository)
    {
        _planReadRepository = planReadRepository;
        _membershipReadRepository = membershipReadRepository;
    }

    public async Task<Result<PlanDetails, Error>> Handle(
        GetPlanByIdQuery request,
        CancellationToken cancellationToken)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Load the plan and count its currently active memberships.
        var data = await _planReadRepository
            .GetAll()
            .Where(plan => plan.Id == request.PlanId)
            .Select(plan => new
            {
                Plan = plan,
                ActiveMembershipsCount = _membershipReadRepository
                    .GetAll()
                    .Count(membership =>
                        membership.PlanId == plan.Id &&
                        membership.Status != MembershipStatus.Cancelled &&
                        membership.StartDate <= today &&
                        membership.EndDate >= today &&
                        !membership.Freezes.Any(freeze =>
                            freeze.StartDate <= today &&
                            freeze.EndDate >= today))
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Return not found when the requested plan does not exist.
        if (data is null)
            return Result.Failure<PlanDetails, Error>(Error.EntityNotFound(nameof(Plan), request.PlanId));

        // Build the Plan Details response.
        PlanDetails response = new(
            data.Plan.Id,
            data.Plan.PlanName,
            data.Plan.Price,
            data.Plan.DurationInMonths,
            data.Plan.MaxFreezeDays,
            data.Plan.MaxFreezes,
            data.Plan.GuestPassQuota,
            data.Plan.AccessScope.Name,
            data.Plan.IsPublished,
            data.ActiveMembershipsCount);

        return Result.Success<PlanDetails, Error>(response);
    }
}