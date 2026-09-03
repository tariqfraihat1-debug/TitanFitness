using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.CheckIns.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.CheckIns.Commands;

public sealed class CreateCheckInCommandHandler
    : IRequestHandler<CreateCheckInCommand, Result<CheckInDetails, Error>>
{
    private readonly IReadOnlyRepository<Member, int> _memberReadRepository;
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;
    private readonly IWriteRepository<CheckIn> _checkInWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCheckInCommandHandler(
        IReadOnlyRepository<Member, int> memberReadRepository,
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IReadOnlyRepository<Membership, int> membershipReadRepository,
        IWriteRepository<CheckIn> checkInWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _memberReadRepository = memberReadRepository;
        _branchReadRepository = branchReadRepository;
        _membershipReadRepository = membershipReadRepository;
        _checkInWriteRepository = checkInWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CheckInDetails, Error>> Handle(
        CreateCheckInCommand request,
        CancellationToken cancellationToken)
    {
        // Load the member and validate the selected branch in one query.
        var data = await _memberReadRepository
            .GetAll()
            .Where(member => member.Id == request.CheckIn.MemberId)
            .Select(member => new
            {
                member.HomeBranchId,
                BranchExists = _branchReadRepository
                    .GetAll()
                    .Any(branch => branch.Id == request.CheckIn.BranchId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
            return Result.Failure<CheckInDetails, Error>(Error.EntityNotFound(nameof(Member), request.CheckIn.MemberId));

        if (!data.BranchExists)
            return Result.Failure<CheckInDetails, Error>(Error.EntityNotFound(nameof(Branch), request.CheckIn.BranchId));

        DateTime now = DateTime.Now;
        DateOnly today = DateOnly.FromDateTime(now);

        // Load the membership that best represents the member's current state.
        Maybe<Membership> membership = await _membershipReadRepository
            .GetAll()
            .Include(membership => membership.Freezes)
            .Where(membership => membership.MemberId == request.CheckIn.MemberId)
            .OrderByDescending(membership =>
                membership.StartDate <= today &&
                membership.EndDate >= today)
            .ThenByDescending(membership => membership.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        CheckInResult result;
        string? refusalReason;

        // Determine whether entry should be admitted or refused.
        if (membership.HasNoValue)
        {
            result = CheckInResult.Refused;
            refusalReason = "Member does not have a membership.";
        }
        else
        {
            MembershipStatus currentStatus = membership.Value.GetStatusOn(today);

            if (currentStatus == MembershipStatus.Cancelled)
            {
                result = CheckInResult.Refused;
                refusalReason = "Membership is cancelled.";
            }
            else if (currentStatus == MembershipStatus.Pending)
            {
                result = CheckInResult.Refused;
                refusalReason = "Membership has not started yet.";
            }
            else if (currentStatus == MembershipStatus.Expired)
            {
                result = CheckInResult.Refused;
                refusalReason = "Membership is expired.";
            }
            else if (currentStatus == MembershipStatus.Frozen)
            {
                result = CheckInResult.Refused;
                refusalReason = "Membership is frozen.";
            }
            else if (
                membership.Value.AgreedTerms.AccessScope == AccessScope.HomeBranchOnly &&
                data.HomeBranchId != request.CheckIn.BranchId)
            {
                result = CheckInResult.Refused;
                refusalReason = "Membership does not allow access to this branch.";
            }
            else
            {
                result = CheckInResult.Admitted;
                refusalReason = null;
            }
        }

        // Create the check-in record with the final decision.
        Result<CheckIn, Error> checkInResult = CheckIn.Create(
            request.CheckIn.MemberId,
            request.CheckIn.BranchId,
            now,
            result,
            refusalReason);

        if (checkInResult.IsFailure)
            return Result.Failure<CheckInDetails, Error>(checkInResult.Error);

        // Persist every check-in attempt, admitted or refused.
        await _checkInWriteRepository.AddAsync(checkInResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return the decision to the UI.
        CheckInDetails response = new(checkInResult.Value.Id,result.Name,refusalReason);

        return Result.Success<CheckInDetails, Error>(response);
    }
}