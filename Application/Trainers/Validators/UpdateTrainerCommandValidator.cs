using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Trainers.Commands;

namespace TitanFitness.Application.Trainers.Validators;

public sealed class UpdateTrainerCommandValidator
    : AbstractValidator<UpdateTrainerCommand>
{
    public UpdateTrainerCommandValidator()
    {
        RuleFor(command => command.TrainerId)
            .ValidId();

        RuleFor(command => command.Trainer.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Trainer.BranchId)
            .ValidId();

        RuleFor(command => command.Trainer.Specialty)
            .MaximumLength(100)
            .When(command => !string.IsNullOrWhiteSpace(command.Trainer.Specialty));

        RuleFor(command => command.Trainer.Email)
            .MaximumLength(150)
            .When(command => !string.IsNullOrWhiteSpace(command.Trainer.Email));
    }
}