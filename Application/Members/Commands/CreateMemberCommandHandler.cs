using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Common.ValueObjects;
using TitanFitness.Domain.Members;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Members.Commands;

public sealed class CreateMemberCommandHandler
    : IRequestHandler<CreateMemberCommand, Result<int, Error>>
{
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IReadOnlyRepository<Member, int> _memberReadRepository;
    private readonly IWriteRepository<Member> _memberWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMemberCommandHandler(
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IReadOnlyRepository<Member, int> memberReadRepository,
        IWriteRepository<Member> memberWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository;
        _memberReadRepository = memberReadRepository;
        _memberWriteRepository = memberWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int, Error>> Handle(
        CreateMemberCommand request,
        CancellationToken cancellationToken)
    {
        // Validate the selected home branch.
        bool branchExists = await _branchReadRepository.AnyAsync(
            branch => branch.Id == request.Member.HomeBranchId,
            cancellationToken);

        if (!branchExists)
            return Result.Failure<int, Error>(Error.EntityNotFound(nameof(Branch), request.Member.HomeBranchId));

        // Use the supplied membership number or generate the next one.
        bool generatedMembershipNumber = string.IsNullOrWhiteSpace(request.Member.MembershipNumber);

        string membershipNumber = generatedMembershipNumber
            ? await GenerateMembershipNumber(cancellationToken)
            : request.Member.MembershipNumber!;

        Result<MembershipNumber, Error> membershipNumberResult = MembershipNumber.Create(membershipNumber);

        if (membershipNumberResult.IsFailure)
            return Result.Failure<int, Error>(membershipNumberResult.Error);

        // Create the optional contact value objects.
        Email? email = null;

        if (!string.IsNullOrWhiteSpace(request.Member.Email))
        {
            Result<Email, Error> emailResult = Email.Create(request.Member.Email);

            if (emailResult.IsFailure)
                return Result.Failure<int, Error>(emailResult.Error);

            email = emailResult.Value;
        }

        Phone? phone = null;

        if (!string.IsNullOrWhiteSpace(request.Member.Phone))
        {
            Result<Phone, Error> phoneResult = Phone.Create(request.Member.Phone);

            if (phoneResult.IsFailure)
                return Result.Failure<int, Error>(phoneResult.Error);

            phone = phoneResult.Value;
        }

        Address? address = null;

        if (!string.IsNullOrWhiteSpace(request.Member.Address))
        {
            Result<Address, Error> addressResult = Address.Create(request.Member.Address);

            if (addressResult.IsFailure)
                return Result.Failure<int, Error>(addressResult.Error);

            address = addressResult.Value;
        }

        // Check uniqueness when the membership number was entered manually.
        if (!generatedMembershipNumber)
        {
            bool membershipNumberExists = await _memberReadRepository.AnyAsync(
                member => member.MembershipNumber.Value == membershipNumberResult.Value.Value,
                cancellationToken);

            if (membershipNumberExists)
                return Result.Failure<int, Error>(Error.EntityAlreadyExists(nameof(Member), nameof(Member.MembershipNumber), membershipNumber));
        }

        // Let the aggregate validate and create the member.
        Result<Member, Error> memberResult = Member.Create(
            membershipNumberResult.Value,
            request.Member.FullName,
            email,
            phone,
            address,
            request.Member.JoinedDate,
            request.Member.Photo,
            request.Member.HomeBranchId);

        if (memberResult.IsFailure)
            return Result.Failure<int, Error>(memberResult.Error);

        // Persist the new member only; membership is purchased separately.
        await _memberWriteRepository.AddAsync(memberResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<int, Error>(memberResult.Value.Id);
    }

    // Generate the next sequential TF membership number.
    private async Task<string> GenerateMembershipNumber(CancellationToken cancellationToken)
    {
        List<string> membershipNumbers = await _memberReadRepository
            .GetAll()
            .Select(member => member.MembershipNumber.Value)
            .ToListAsync(cancellationToken);

        int maxNumber = membershipNumbers
            .Where(number => number.StartsWith("TF-"))
            .Select(number => int.TryParse(number.Replace("TF-", ""), out int value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"TF-{maxNumber + 1:D4}";
    }
}