using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Common.ValueObjects;

public sealed class Id : ValueObject
{
    public int Value { get; private set; }

    private Id()
    {
    }

    private Id(int value)
    {
        Value = value;
    }

    public static Result<Id, Error> Create(int value)
    {
        if (value <= 0)
            return Result.Failure<Id, Error>(Error.InvalidValue(nameof(Value),value));
        

        Id id = new Id(value);

        return Result.Success<Id, Error>(id);
    }

    protected override IEnumerable<object>
        GetEqualityComponents()
    {
        yield return Value;
    }
}