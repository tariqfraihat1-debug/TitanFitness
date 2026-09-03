using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Common.ValueObjects;

namespace TitanFitness.Domain.Branches;

public sealed class Branch : TitanFitness.Domain.Common.Entities.Entity<int>, IAggregateRoot
{
    private readonly List<Studio> _studios = [];

    public string Name { get; private set; } = null!;
    public Address? Address { get; private set; }
    public TimeOnly OpeningTime { get; private set; }
    public TimeOnly ClosingTime { get; private set; }
    public IReadOnlyList<Studio> Studios => _studios;

    private Branch()
    {
    }

    private Branch(
        string name,
        Address? address,
        TimeOnly openingTime,
        TimeOnly closingTime)
    {
        Name = name;
        Address = address;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
    }

    public static Result<Branch, Error> Create(
        string name,
        Address? address,
        TimeOnly openingTime,
        TimeOnly closingTime)
    {
        // Validate the branch details.
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Branch, Error>(Error.ValueIsRequired(nameof(Name)));

        if (name.Length > 50)
            return Result.Failure<Branch, Error>(Error.ExceedMaxLength(nameof(Name), 50));

        if (closingTime <= openingTime)
            return Result.Failure<Branch, Error>(Error.InvalidOperation("Closing time must be after opening time."));

        Branch branch = new(name.Trim(), address, openingTime, closingTime);

        return Result.Success<Branch, Error>(branch);
    }

    public UnitResult<Error> Update(
        string name,
        Address? address,
        TimeOnly openingTime,
        TimeOnly closingTime)
    {
        // Validate the updated branch details.
        if (string.IsNullOrWhiteSpace(name))
            return UnitResult.Failure(Error.ValueIsRequired(nameof(Name)));

        if (name.Length > 50)
            return UnitResult.Failure(Error.ExceedMaxLength(nameof(Name), 50));

        if (closingTime <= openingTime)
            return UnitResult.Failure(Error.InvalidOperation("Closing time must be after opening time."));

        Name = name.Trim();
        Address = address;
        OpeningTime = openingTime;
        ClosingTime = closingTime;

        return UnitResult.Success<Error>();
    }

    public Result<Studio, Error> AddStudio(string name, int capacity)
    {
        // Create and attach the studio to this branch.
        Result<Studio, Error> studioResult = Studio.Create(name, capacity);

        if (studioResult.IsFailure)
            return studioResult;

        _studios.Add(studioResult.Value);

        return Result.Success<Studio, Error>(studioResult.Value);
    }
}