using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Common.ValueObjects;

namespace TitanFitness.Domain.Trainers;

public sealed class Trainer : TitanFitness.Domain.Common.Entities.Entity<int>, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public int BranchId { get; private set; }
    public string? Specialty { get; private set; }
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }
    public bool IsActive { get; private set; }

    private Trainer()
    {
    }

    private Trainer(
        string name,
        int branchId,
        string? specialty,
        Email? email,
        Phone? phone,
        bool isActive)
    {
        Name = name;
        BranchId = branchId;
        Specialty = specialty;
        Email = email;
        Phone = phone;
        IsActive = isActive;
    }

    public static Result<Trainer, Error> Create(
        string name,
        int branchId,
        string? specialty,
        Email? email,
        Phone? phone,
        bool isActive)
    {
        // Validate the trainer details.
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Trainer, Error>(Error.ValueIsRequired(nameof(Name)));

        if (name.Length > 100)
            return Result.Failure<Trainer, Error>(Error.ExceedMaxLength(nameof(Name), 100));

        if (specialty?.Length > 100)
            return Result.Failure<Trainer, Error>(Error.ExceedMaxLength(nameof(Specialty), 100));

        Trainer trainer = new(
            name.Trim(),
            branchId,
            specialty?.Trim(),
            email,
            phone,
            isActive);

        return Result.Success<Trainer, Error>(trainer);
    }

    public UnitResult<Error> Update(
        string name,
        int branchId,
        string? specialty,
        Email? email,
        Phone? phone,
        bool isActive)
    {
        // Validate the updated trainer details.
        if (string.IsNullOrWhiteSpace(name))
            return UnitResult.Failure(Error.ValueIsRequired(nameof(Name)));

        if (name.Length > 100)
            return UnitResult.Failure(Error.ExceedMaxLength(nameof(Name), 100));

        if (specialty?.Length > 100)
            return UnitResult.Failure(Error.ExceedMaxLength(nameof(Specialty), 100));

        Name = name.Trim();
        BranchId = branchId;
        Specialty = specialty?.Trim();
        Email = email;
        Phone = phone;
        IsActive = isActive;

        return UnitResult.Success<Error>();
    }
}