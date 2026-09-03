using FluentValidation;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.Memberships.Commands;

public sealed class CreateMembershipCommandValidator
    : AbstractValidator<CreateMembershipCommand>
{
    public CreateMembershipCommandValidator()
    {
        RuleFor(x => x.Membership.MemberId).ValidId();

        RuleFor(x => x.Membership.PlanId).ValidId();

        RuleFor(x => x.Membership.StartDate)
            .NotEmpty();
    }
}