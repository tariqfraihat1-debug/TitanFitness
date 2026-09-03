using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Members.Queries;

namespace TitanFitness.Application.Members.Validators;

public sealed class GetMemberByIdQueryValidator
    : AbstractValidator<GetMemberByIdQuery>
{
    public GetMemberByIdQueryValidator()
    {
        RuleFor(query => query.MemberId)
            .ValidId();
    }
}