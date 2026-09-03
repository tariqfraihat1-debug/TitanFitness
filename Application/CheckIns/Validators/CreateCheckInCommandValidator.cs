using FluentValidation;
using TitanFitness.Application.CheckIns.Commands;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.CheckIns.Validators;

public sealed class CreateCheckInCommandValidator
    : AbstractValidator<CreateCheckInCommand>
{
    public CreateCheckInCommandValidator()
    {
        RuleFor(command => command.CheckIn.MemberId)
            .ValidId();

        RuleFor(command => command.CheckIn.BranchId)
            .ValidId();
    }
}