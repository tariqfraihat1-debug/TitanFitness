using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Memberships.Queries;

namespace TitanFitness.Application.Memberships.Validators;

public sealed class GetMembershipByIdQueryValidator
    : AbstractValidator<GetMembershipByIdQuery>
{
    public GetMembershipByIdQueryValidator()
    {
        RuleFor(query => query.MembershipId)
            .ValidId();
    }
}