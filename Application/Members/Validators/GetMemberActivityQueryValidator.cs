using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Members.Queries;

namespace TitanFitness.Application.Members.Validators;

public sealed class GetMemberActivityQueryValidator
    : AbstractValidator<GetMemberActivityQuery>
{
    public GetMemberActivityQueryValidator()
    {
        RuleFor(query => query.MemberId)
            .ValidId();

    }
}