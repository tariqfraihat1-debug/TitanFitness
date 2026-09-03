using FluentValidation;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.Trainers.Commands;

public sealed class CreateTrainerCommandValidator
    : AbstractValidator<CreateTrainerCommand>
{
    public CreateTrainerCommandValidator()
    {
        RuleFor(x => x.Trainer.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Trainer.BranchId)
            .ValidId();

        RuleFor(x => x.Trainer.Specialty)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Trainer.Specialty));

        RuleFor(x => x.Trainer.Email)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Trainer.Email));

        RuleFor(x => x.Trainer.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Trainer.Phone));
    }
}