using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Memberships.Commands;

public sealed class CreateMembershipCommandHandler
    : IRequestHandler<CreateMembershipCommand, Result<int, Error>>
{
    private readonly IReadOnlyRepository<Member, int> _memberReadRepository;
    private readonly IReadOnlyRepository<Plan, int> _planReadRepository;
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;
    private readonly IWriteRepository<Membership> _membershipWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMembershipCommandHandler(
        IReadOnlyRepository<Member, int> memberReadRepository,
        IReadOnlyRepository<Plan, int> planReadRepository,
        IReadOnlyRepository<Membership, int> membershipReadRepository,
        IWriteRepository<Membership> membershipWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _memberReadRepository = memberReadRepository;
        _planReadRepository = planReadRepository;
        _membershipReadRepository = membershipReadRepository;
        _membershipWriteRepository = membershipWriteRepository;
        _unitOfWork = unitOfWork;
    }

    // Creates a membership when the member and plan are valid.
    public async Task<Result<int, Error>> Handle(
        CreateMembershipCommand request,
        CancellationToken cancellationToken)
    {
        Maybe<Member> member = await _memberReadRepository.FirstOrDefaultAsync(
            member => member.Id == request.Membership.MemberId,
            cancellationToken);

        if (member.HasNoValue)
            return Result.Failure<int, Error>(
                Error.EntityNotFound(nameof(Member), request.Membership.MemberId));

        Maybe<Plan> plan = await _planReadRepository.FirstOrDefaultAsync(
            plan => plan.Id == request.Membership.PlanId,
            cancellationToken);

        if (plan.HasNoValue)
            return Result.Failure<int, Error>(
                Error.EntityNotFound(nameof(Plan), request.Membership.PlanId));

        if (!plan.Value.IsPublished)
            return Result.Failure<int, Error>(
                Error.InvalidOperation("Only a published plan can be purchased."));

        Result<AgreedTerms, Error> agreedTermsResult = AgreedTerms.Create(
            plan.Value.Price,
            plan.Value.DurationInMonths,
            plan.Value.MaxFreezeDays,
            plan.Value.MaxFreezes,
            plan.Value.GuestPassQuota,
            plan.Value.AccessScope);

        if (agreedTermsResult.IsFailure)
            return Result.Failure<int, Error>(agreedTermsResult.Error);

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        Result<Membership, Error> membershipResult = Membership.Create(
            request.Membership.MemberId,
            request.Membership.PlanId,
            DateTime.Now,
            request.Membership.StartDate,
            agreedTermsResult.Value,
            today);

        if (membershipResult.IsFailure)
            return Result.Failure<int, Error>(membershipResult.Error);

        Membership newMembership = membershipResult.Value;

        bool hasOverlap = await _membershipReadRepository.AnyAsync(
            membership =>
                membership.MemberId == request.Membership.MemberId &&
                membership.Status != MembershipStatus.Cancelled &&
                membership.StartDate <= newMembership.EndDate &&
                membership.EndDate >= newMembership.StartDate,
            cancellationToken);

        if (hasOverlap)
            return Result.Failure<int, Error>(
                Error.InvalidOperation(
                    "Member already has a membership that overlaps the requested period."));

        await _membershipWriteRepository.AddAsync(
            newMembership,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<int, Error>(newMembership.Id);
    }
}