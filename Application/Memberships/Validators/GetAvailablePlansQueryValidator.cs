using FluentValidation;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.Memberships.Queries;

public sealed class GetAvailablePlansQueryValidator
    : AbstractValidator<GetAvailablePlansQuery>
{
    public GetAvailablePlansQueryValidator()
    {
        RuleFor(x => x.MembershipId).ValidId();
    }
}