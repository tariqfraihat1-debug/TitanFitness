using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Common.ValueObjects;
using TitanFitness.Domain.Members;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Members.Commands;

public sealed class UpdateMemberCommandHandler
    : IRequestHandler<UpdateMemberCommand, UnitResult<Error>>
{
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IWriteRepository<Member> _memberWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberCommandHandler(
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IWriteRepository<Member> memberWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository;
        _memberWriteRepository = memberWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<Error>> Handle(
        UpdateMemberCommand request,
        CancellationToken cancellationToken)
    {
        // Load the tracked member that will be updated.
        Maybe<Member> member = await _memberWriteRepository.FindAsync(
            [request.MemberId],
            cancellationToken);

        if (member.HasNoValue)
            return UnitResult.Failure(Error.EntityNotFound(nameof(Member), request.MemberId));

        // Validate the selected home branch.
        bool branchExists = await _branchReadRepository.AnyAsync(
            branch => branch.Id == request.Member.HomeBranchId,
            cancellationToken);

        if (!branchExists)
            return UnitResult.Failure(Error.EntityNotFound(nameof(Branch), request.Member.HomeBranchId));

        // Create the optional contact value objects.
        Email? email = null;

        if (!string.IsNullOrWhiteSpace(request.Member.Email))
        {
            Result<Email, Error> emailResult = Email.Create(request.Member.Email);

            if (emailResult.IsFailure)
                return UnitResult.Failure(emailResult.Error);

            email = emailResult.Value;
        }

        Phone? phone = null;

        if (!string.IsNullOrWhiteSpace(request.Member.Phone))
        {
            Result<Phone, Error> phoneResult = Phone.Create(request.Member.Phone);

            if (phoneResult.IsFailure)
                return UnitResult.Failure(phoneResult.Error);

            phone = phoneResult.Value;
        }

        Address? address = null;

        if (!string.IsNullOrWhiteSpace(request.Member.Address))
        {
            Result<Address, Error> addressResult = Address.Create(request.Member.Address);

            if (addressResult.IsFailure)
                return UnitResult.Failure(addressResult.Error);

            address = addressResult.Value;
        }

        // Update only the member profile fields; membership data stays unchanged.
        UnitResult<Error> updateResult = member.Value.Update(
            request.Member.FullName,
            email,
            phone,
            address,
            request.Member.JoinedDate,
            request.Member.Photo,
            request.Member.HomeBranchId);

        if (updateResult.IsFailure)
            return UnitResult.Failure(updateResult.Error);

        // Persist the tracked member changes.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}