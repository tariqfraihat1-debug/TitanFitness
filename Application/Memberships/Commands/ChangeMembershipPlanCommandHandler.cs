using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Memberships.Contract;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Memberships.Commands;

public sealed class ChangeMembershipPlanCommandHandler
    : IRequestHandler<ChangeMembershipPlanCommand, Result<int, Error>>
{
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;
    private readonly IReadOnlyRepository<Plan, int> _planReadRepository;
    private readonly IWriteRepository<Membership> _membershipWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeMembershipPlanCommandHandler(
        IReadOnlyRepository<Membership, int> membershipReadRepository,
        IReadOnlyRepository<Plan, int> planReadRepository,
        IWriteRepository<Membership> membershipWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipReadRepository = membershipReadRepository;
        _planReadRepository = planReadRepository;
        _membershipWriteRepository = membershipWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int, Error>> Handle(
        ChangeMembershipPlanCommand request,
        CancellationToken cancellationToken)
    {
        // Load the tracked current membership and selected plan in one query.
        var data = await (
            from membership in _membershipWriteRepository.Query()
            from plan in _planReadRepository.GetAll()
            where membership.Id == request.MembershipId &&
                  plan.Id == request.Plan.NewPlanId
            select new
            {
                Membership = membership,
                Plan = plan
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Return the specific missing entity error when setup is invalid.
        if (data is null)
        {
            bool membershipExists = await _membershipReadRepository.AnyAsync(
                membership => membership.Id == request.MembershipId,
                cancellationToken);

            if (!membershipExists)
                return Result.Failure<int, Error>(Error.EntityNotFound(nameof(Membership), request.MembershipId));

            return Result.Failure<int, Error>(Error.EntityNotFound(nameof(Plan), request.Plan.NewPlanId));
        }

        if (data.Membership.Status == MembershipStatus.Cancelled)
            return Result.Failure<int, Error>(Error.InvalidOperation("Cancelled memberships cannot be changed."));

        if (!data.Plan.IsPublished)
            return Result.Failure<int, Error>(Error.InvalidOperation("Only a published plan can be selected."));

        DateTime now = DateTime.Now;
        DateOnly today = DateOnly.FromDateTime(now);

        // Determine the new membership start date from the selected mode.
        DateOnly startDate;

        if (request.Plan.EffectiveMode == ChangePlanEffectiveMode.Immediately)
        {
            UnitResult<Error> cancelResult = data.Membership.Cancel();

            if (cancelResult.IsFailure)
                return Result.Failure<int, Error>(cancelResult.Error);

            startDate = today;
        }
else if (request.Plan.EffectiveMode == ChangePlanEffectiveMode.AtRenewal)
{
    startDate = data.Membership.EndDate < today
        ? today
        : data.Membership.EndDate.AddDays(1);
}
        else
        {
            return Result.Failure<int, Error>(Error.InvalidValue(nameof(ChangePlanEffectiveMode), request.Plan.EffectiveMode));
        }

        // Snapshot the selected plan terms for the new membership.
        Result<AgreedTerms, Error> agreedTermsResult = AgreedTerms.Create(
            data.Plan.Price,
            data.Plan.DurationInMonths,
            data.Plan.MaxFreezeDays,
            data.Plan.MaxFreezes,
            data.Plan.GuestPassQuota,
            data.Plan.AccessScope);

        if (agreedTermsResult.IsFailure)
            return Result.Failure<int, Error>(agreedTermsResult.Error);

        DateOnly endDate = startDate
            .AddMonths(agreedTermsResult.Value.DurationInMonths)
            .AddDays(-1);

        // Prevent the member from holding overlapping memberships.
        bool overlaps = await _membershipReadRepository.AnyAsync(
            membership =>
                membership.MemberId == data.Membership.MemberId &&
                membership.Id != data.Membership.Id &&
                membership.Status != MembershipStatus.Cancelled &&
                membership.StartDate <= endDate &&
                membership.EndDate >= startDate,
            cancellationToken);

        if (overlaps)
            return Result.Failure<int, Error>(Error.InvalidOperation("The new membership overlaps an existing membership."));

        // Create the new membership from the selected plan terms.
        Result<Membership, Error> membershipResult = Membership.Create(
            data.Membership.MemberId,
            data.Plan.Id,
            now,
            startDate,
            agreedTermsResult.Value,
            today);

        if (membershipResult.IsFailure)
            return Result.Failure<int, Error>(membershipResult.Error);

        // Persist the new membership and any immediate cancellation.
        await _membershipWriteRepository.AddAsync(membershipResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<int, Error>(membershipResult.Value.Id);
    }
}