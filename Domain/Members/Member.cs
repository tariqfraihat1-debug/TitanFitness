using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Common.ValueObjects;

namespace TitanFitness.Domain.Members;

public sealed class Member : TitanFitness.Domain.Common.Entities.Entity<int>, IAggregateRoot
{
    public MembershipNumber MembershipNumber { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }
    public Address? Address { get; private set; }
    public DateOnly JoinedDate { get; private set; }
    public string? Photo { get; private set; }
    public int HomeBranchId { get; private set; }

    private Member()
    {
    }

    private Member(
        MembershipNumber membershipNumber,
        string fullName,
        Email? email,
        Phone? phone,
        Address? address,
        DateOnly joinedDate,
        string? photo,
        int homeBranchId)
    {
        MembershipNumber = membershipNumber;
        FullName = fullName;
        Email = email;
        Phone = phone;
        Address = address;
        JoinedDate = joinedDate;
        Photo = photo;
        HomeBranchId = homeBranchId;
    }

    public static Result<Member, Error> Create(
        MembershipNumber membershipNumber,
        string fullName,
        Email? email,
        Phone? phone,
        Address? address,
        DateOnly joinedDate,
        string? photo,
        int homeBranchId)
    {
        // Validate the required member details.
        if (membershipNumber is null)
            return Result.Failure<Member, Error>(Error.ValueIsRequired(nameof(MembershipNumber)));

        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure<Member, Error>(Error.ValueIsRequired(nameof(FullName)));

        if (fullName.Length > 100)
            return Result.Failure<Member, Error>(Error.ExceedMaxLength(nameof(FullName), 100));

        // Create the member with the supplied profile information.
        Member member = new(
            membershipNumber,
            fullName.Trim(),
            email,
            phone,
            address,
            joinedDate,
            photo,
            homeBranchId);

        return Result.Success<Member, Error>(member);
    }

    public UnitResult<Error> Update(
        string fullName,
        Email? email,
        Phone? phone,
        Address? address,
        DateOnly joinedDate,
        string? photo,
        int homeBranchId)
    {
        // Validate the editable member details.
        if (string.IsNullOrWhiteSpace(fullName))
            return UnitResult.Failure(Error.ValueIsRequired(nameof(FullName)));

        if (fullName.Length > 100)
            return UnitResult.Failure(Error.ExceedMaxLength(nameof(FullName), 100));

        // Update the profile while keeping the membership number unchanged.
        FullName = fullName.Trim();
        Email = email;
        Phone = phone;
        Address = address;
        JoinedDate = joinedDate;
        Photo = photo;
        HomeBranchId = homeBranchId;

        return UnitResult.Success<Error>();
    }
}