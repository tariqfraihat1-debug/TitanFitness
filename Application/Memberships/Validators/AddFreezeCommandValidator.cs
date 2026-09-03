using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Memberships.Commands;

namespace TitanFitness.Application.Memberships.Validators;

public sealed class AddFreezeCommandValidator
    : AbstractValidator<AddFreezeCommand>
{
    public AddFreezeCommandValidator()
    {
        RuleFor(command => command.MembershipId)
            .ValidId();

        RuleFor(command => command.Freeze.StartDate)
            .NotEmpty();

        RuleFor(command => command.Freeze.FreezeDurationId)
            .ValidId();

        RuleFor(command => command.Freeze.FreezeReasonId)
            .ValidId();

        RuleFor(command => command.Freeze.Notes)
            .MaximumLength(200)
            .When(command => !string.IsNullOrWhiteSpace(command.Freeze.Notes));
    }
}