using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Members.Queries;

public sealed class GetEntryEligibilityQueryHandler
    : IRequestHandler<GetEntryEligibilityQuery, Result<EntryEligibility, Error>>
{
    private readonly IReadOnlyRepository<Member, int> _memberReadRepository;
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;

    public GetEntryEligibilityQueryHandler(
        IReadOnlyRepository<Member, int> memberReadRepository,
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IReadOnlyRepository<Membership, int> membershipReadRepository)
    {
        _memberReadRepository = memberReadRepository;
        _branchReadRepository = branchReadRepository;
        _membershipReadRepository = membershipReadRepository;
    }

    public async Task<Result<EntryEligibility, Error>> Handle(
        GetEntryEligibilityQuery request,
        CancellationToken cancellationToken)
    {
        // Load the member and validate the requested branch in one query.
        var data = await _memberReadRepository
            .GetAll()
            .Where(member => member.Id == request.MemberId)
            .Select(member => new
            {
                member.HomeBranchId,
                BranchExists = _branchReadRepository
                    .GetAll()
                    .Any(branch => branch.Id == request.BranchId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
            return Result.Failure<EntryEligibility, Error>(Error.EntityNotFound(nameof(Member), request.MemberId));

        if (!data.BranchExists)
            return Result.Failure<EntryEligibility, Error>(Error.EntityNotFound(nameof(Branch), request.BranchId));

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        // Load the membership most relevant to today with its freezes.
        Maybe<Membership> membership = await _membershipReadRepository
            .GetAll()
            .Include(membership => membership.Freezes)
            .Where(membership => membership.MemberId == request.MemberId)
            .OrderByDescending(membership =>
                membership.StartDate <= today &&
                membership.EndDate >= today)
            .ThenByDescending(membership => membership.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (membership.HasNoValue)
            return Result.Success<EntryEligibility, Error>(new EntryEligibility(false, "Member does not have a membership."));

        // Calculate the live membership status using the Domain rule.
        MembershipStatus currentStatus = membership.Value.GetStatusOn(today);

        if (currentStatus == MembershipStatus.Cancelled)
            return Result.Success<EntryEligibility, Error>(new EntryEligibility(false, "Membership is cancelled."));

        if (currentStatus == MembershipStatus.Pending)
            return Result.Success<EntryEligibility, Error>(new EntryEligibility(false, "Membership has not started yet."));

        if (currentStatus == MembershipStatus.Expired)
            return Result.Success<EntryEligibility, Error>(new EntryEligibility(false, "Membership is expired."));

        if (currentStatus == MembershipStatus.Frozen)
            return Result.Success<EntryEligibility, Error>(new EntryEligibility(false, "Membership is currently frozen."));

        // Enforce home-branch-only access after confirming the membership is active.
        if (membership.Value.AgreedTerms.AccessScope == AccessScope.HomeBranchOnly &&
            data.HomeBranchId != request.BranchId)
            return Result.Success<EntryEligibility, Error>(new EntryEligibility(false, "Membership only allows access to the home branch."));

        return Result.Success<EntryEligibility, Error>(new EntryEligibility(true, null));
    }
}