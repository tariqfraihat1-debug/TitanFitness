using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Memberships.Commands;

public sealed class RenewMembershipCommandHandler
    : IRequestHandler<RenewMembershipCommand, Result<int, Error>>
{
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;
    private readonly IReadOnlyRepository<Plan, int> _planReadRepository;
    private readonly IWriteRepository<Membership> _membershipWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RenewMembershipCommandHandler(
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
        RenewMembershipCommand request,
        CancellationToken cancellationToken)
    {
        // Load the membership and its plan in one query.
        var data = await (
            from membership in _membershipReadRepository.GetAll()
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
            return Result.Failure<int, Error>(Error.EntityNotFound(nameof(Membership), request.MembershipId));

        DateTime now = DateTime.Now;
        DateOnly today = DateOnly.FromDateTime(now);

        // Only expired, non-cancelled memberships can be renewed.
        if (data.Membership.Status == MembershipStatus.Cancelled)
            return Result.Failure<int, Error>(Error.InvalidOperation("Cancelled memberships cannot be renewed."));

        if (data.Membership.EndDate >= today)
            return Result.Failure<int, Error>(Error.InvalidOperation("Only an expired membership can be renewed."));

        // The plan must still be available.
        if (!data.Plan.IsPublished)
            return Result.Failure<int, Error>(Error.InvalidOperation("The current plan is not available for renewal."));

        // Snapshot the current plan terms for the new membership.
        Result<AgreedTerms, Error> agreedTermsResult = AgreedTerms.Create(
            data.Plan.Price,
            data.Plan.DurationInMonths,
            data.Plan.MaxFreezeDays,
            data.Plan.MaxFreezes,
            data.Plan.GuestPassQuota,
            data.Plan.AccessScope);

        if (agreedTermsResult.IsFailure)
            return Result.Failure<int, Error>(agreedTermsResult.Error);

        DateOnly startDate = today;
        DateOnly endDate = startDate
            .AddMonths(agreedTermsResult.Value.DurationInMonths)
            .AddDays(-1);

        // Prevent overlapping memberships.
        bool overlaps = await _membershipReadRepository.AnyAsync(
            membership =>
                membership.MemberId == data.Membership.MemberId &&
                membership.Id != data.Membership.Id &&
                membership.Status != MembershipStatus.Cancelled &&
                membership.StartDate <= endDate &&
                membership.EndDate >= startDate,
            cancellationToken);

        if (overlaps)
            return Result.Failure<int, Error>(Error.InvalidOperation("The renewed membership overlaps an existing membership."));

        // Create a new membership using today's plan terms.
        Result<Membership, Error> membershipResult = Membership.Create(
            data.Membership.MemberId,
            data.Plan.Id,
            now,
            startDate,
            agreedTermsResult.Value,
            today);

        if (membershipResult.IsFailure)
            return Result.Failure<int, Error>(membershipResult.Error);

        // Save the new membership.
        await _membershipWriteRepository.AddAsync(membershipResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<int, Error>(membershipResult.Value.Id);
    }
}