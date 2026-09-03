using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Branches;

public sealed class Studio : TitanFitness.Domain.Common.Entities.Entity<int>
{
    public string Name { get; private set; } = null!;
    public int BranchId { get; internal set; }
    public int Capacity { get; private set; }

    private Studio()
    {
    }

    private Studio(string name, int capacity)
    {
        Name = name;
        Capacity = capacity;
    }

    internal static Result<Studio, Error> Create(string name, int capacity)
    {
        // Validate the studio details.
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Studio, Error>(Error.ValueIsRequired(nameof(Name)));

        if (name.Length > 50)
            return Result.Failure<Studio, Error>(Error.ExceedMaxLength(nameof(Name), 50));

        if (capacity <= 0)
            return Result.Failure<Studio, Error>(Error.InvalidValue(nameof(Capacity), capacity));

        // Create the studio; Branch assigns the relationship.
        Studio studio = new(name.Trim(), capacity);

        return Result.Success<Studio, Error>(studio);
    }
}