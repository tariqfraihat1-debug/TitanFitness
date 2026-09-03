using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Memberships.Commands;
using TitanFitness.Application.Memberships.Contract;

namespace TitanFitness.Application.Memberships.Validators;

public sealed class ChangeMembershipPlanCommandValidator
    : AbstractValidator<ChangeMembershipPlanCommand>
{
    public ChangeMembershipPlanCommandValidator()
    {
        RuleFor(command => command.MembershipId)
            .ValidId();

        RuleFor(command => command.Plan.NewPlanId)
            .ValidId();

        RuleFor(command => command.Plan.EffectiveMode)
            .IsInEnum();
    }
}