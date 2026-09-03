using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Members;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Members.Queries;

public sealed class GetMemberByIdQueryHandler
    : IRequestHandler<GetMemberByIdQuery, Result<MemberDetails, Error>>
{
    private readonly IReadOnlyRepository<Member, int> _memberReadRepository;

    public GetMemberByIdQueryHandler(
        IReadOnlyRepository<Member, int> memberReadRepository)
    {
        _memberReadRepository = memberReadRepository;
    }

    public async Task<Result<MemberDetails, Error>> Handle(
        GetMemberByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Load only the member details required by the profile.
        Maybe<MemberDetails> member = await _memberReadRepository
            .GetAll()
            .Where(member => member.Id == request.MemberId)
            .Select(member => new MemberDetails(
                member.Id,
                member.MembershipNumber.Value,
                member.FullName,
                member.Email != null ? member.Email.Value : null,
                member.Phone != null ? member.Phone.Value : null,
                member.Address != null ? member.Address.Value : null,
                member.JoinedDate,
                member.Photo,
                member.HomeBranchId))
            .FirstOrDefaultAsync(cancellationToken);

        if (member.HasNoValue)
            return Result.Failure<MemberDetails, Error>(Error.EntityNotFound(nameof(Member), request.MemberId));

        return Result.Success<MemberDetails, Error>(member.Value);
    }
}