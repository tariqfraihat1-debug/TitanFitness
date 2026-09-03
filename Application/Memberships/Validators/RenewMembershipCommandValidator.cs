using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Memberships.Commands;

namespace TitanFitness.Application.Memberships.Validators;

public sealed class RenewMembershipCommandValidator
    : AbstractValidator<RenewMembershipCommand>
{
    public RenewMembershipCommandValidator()
    {
        RuleFor(command => command.MembershipId)
            .ValidId();
    }
}