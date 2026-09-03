using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Members.Queries;

namespace TitanFitness.Application.Members.Validators;

public sealed class GetEntryEligibilityQueryValidator
    : AbstractValidator<GetEntryEligibilityQuery>
{
    public GetEntryEligibilityQueryValidator()
    {
        RuleFor(query => query.MemberId)
            .ValidId();

        RuleFor(query => query.BranchId)
            .ValidId();
    }
}