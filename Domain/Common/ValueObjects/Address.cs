using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Common.ValueObjects;

public sealed class Address : ValueObject
{
    public string Value { get; private set; } = null!;

    private Address()
    {
    }

    private Address(string value)
    {
        Value = value;
    }

    public static Result<Address, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Address, Error>(Error.ValueIsRequired(nameof(Address)));

        if (value.Length > 200)
            return Result.Failure<Address, Error>(Error.ExceedMaxLength(nameof(Address), 200));

        Address address = new Address(value.Trim());
        return Result.Success<Address, Error>(address);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}