using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Common.ValueObjects;

public sealed class Phone : ValueObject
{
    public string Value { get; private set; } = null!;

    private Phone()
    {
    }

    private Phone(string value)
    {
        Value = value;
    }

    public static Result<Phone, Error> Create(string value)
    {
        // Validate the phone number format.
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Phone, Error>(Error.ValueIsRequired(nameof(Phone)));

        string trimmedPhone = value.Trim();

        if (trimmedPhone.Length > 20)
            return Result.Failure<Phone, Error>(Error.ExceedMaxLength(nameof(Phone), 20));

        if (!Regex.IsMatch(trimmedPhone, @"^\+?[0-9]+(-[0-9]+)*$"))
            return Result.Failure<Phone, Error>(Error.InvalidFieldFormat(nameof(Phone), value));

        Phone phone = new(trimmedPhone);

        return Result.Success<Phone, Error>(phone);
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