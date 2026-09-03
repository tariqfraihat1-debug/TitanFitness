using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Common.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; private set; } = null!;

    private Email()
    {
    }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email, Error> Create(string value)
    {
        // Validate and normalize the email.
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email, Error>(Error.ValueIsRequired(nameof(Email)));

        string normalizedEmail = value.Trim().ToLowerInvariant();

        if (normalizedEmail.Length > 100)
            return Result.Failure<Email, Error>(Error.ExceedMaxLength(nameof(Email), 100));

        if (!Regex.IsMatch(normalizedEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Result.Failure<Email, Error>(Error.InvalidFieldFormat(nameof(Email), value));

        Email email = new(normalizedEmail);

        return Result.Success<Email, Error>(email);
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