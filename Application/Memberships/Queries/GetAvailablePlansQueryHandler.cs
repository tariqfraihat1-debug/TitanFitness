using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Plans.Contract;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Memberships.Queries;

public sealed class GetAvailablePlansQueryHandler
    : IRequestHandler<GetAvailablePlansQuery, Result<List<PlanLookupItem>, Error>>
{
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;
    private readonly IReadOnlyRepository<Plan, int> _planReadRepository;

    public GetAvailablePlansQueryHandler(
        IReadOnlyRepository<Membership, int> membershipReadRepository,
        IReadOnlyRepository<Plan, int> planReadRepository)
    {
        _membershipReadRepository = membershipReadRepository;
        _planReadRepository = planReadRepository;
    }

    public async Task<Result<List<PlanLookupItem>, Error>> Handle(
        GetAvailablePlansQuery request,
        CancellationToken cancellationToken)
    {
        // Validate the membership exists before loading switch options.
        bool membershipExists = await _membershipReadRepository.AnyAsync(
            membership => membership.Id == request.MembershipId,
            cancellationToken);

        if (!membershipExists)
            return Result.Failure<List<PlanLookupItem>, Error>(Error.EntityNotFound(nameof(Membership), request.MembershipId));

        // Only published plans can be selected for a membership change.
        List<PlanLookupItem> plans = await _planReadRepository
            .GetAll()
            .Where(plan => plan.IsPublished)
            .OrderBy(plan => plan.PlanName)
            .Select(plan => new PlanLookupItem(
                plan.Id,
                plan.PlanName,
                plan.Price,
                plan.DurationInMonths))
            .ToListAsync(cancellationToken);

        return Result.Success<List<PlanLookupItem>, Error>(plans);
    }
}