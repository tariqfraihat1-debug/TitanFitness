using CSharpFunctionalExtensions;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Domain.Common.ValueObjects;

public sealed class MembershipNumber : ValueObject
{
    public string Value { get; private set; } = null!;

    private MembershipNumber()
    {
    }

    private MembershipNumber(string value)
    {
        Value = value;
    }

    public static Result<MembershipNumber, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<MembershipNumber, Error>(Error.ValueIsRequired(nameof(MembershipNumber)));

        if (value.Length > 10)
            return Result.Failure<MembershipNumber, Error>(Error.ExceedMaxLength(nameof(MembershipNumber), 10));

        MembershipNumber membershipNumber = new MembershipNumber(value.Trim().ToUpperInvariant());
        return Result.Success<MembershipNumber, Error>(membershipNumber);
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