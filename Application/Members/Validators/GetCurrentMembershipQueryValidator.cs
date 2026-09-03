using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Members.Queries;

namespace TitanFitness.Application.Members.Validators;

public sealed class GetCurrentMembershipQueryValidator
    : AbstractValidator<GetCurrentMembershipQuery>
{
    public GetCurrentMembershipQueryValidator()
    {
        RuleFor(query => query.MemberId)
            .ValidId();
    }
}