using FluentValidation;
using TitanFitness.Application.ClassSessions.Commands;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.ClassSessions.Validators;

public sealed class CreateClassSessionCommandValidator
    : AbstractValidator<CreateClassSessionCommand>
{
    public CreateClassSessionCommandValidator()
    {
        RuleFor(command => command.ClassSession.ClassName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.ClassSession.BranchId)
            .ValidId();

        RuleFor(command => command.ClassSession.StudioId)
            .ValidId();

        RuleFor(command => command.ClassSession.TrainerId)
            .ValidId();

        RuleFor(command => command.ClassSession.SessionDate)
            .NotEmpty();

        RuleFor(command => command.ClassSession.StartTime)
            .NotEmpty();

        RuleFor(command => command.ClassSession.DurationMinutes)
            .Must(duration => duration is 30 or 45 or 60)
            .WithMessage("DurationMinutes must be 30, 45, or 60.");

        RuleFor(command => command.ClassSession.CapacityLimit)
            .GreaterThan(0);

        RuleFor(command => command.ClassSession.Description)
            .MaximumLength(500)
            .When(command => !string.IsNullOrWhiteSpace(command.ClassSession.Description));
    }
}