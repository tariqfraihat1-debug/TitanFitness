using FluentValidation;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Application.Plans.Commands;

public sealed class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Plan.PlanName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Plan.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Plan.DurationInMonths)
            .GreaterThan(0);

        RuleFor(x => x.Plan.MaxFreezeDays)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Plan.MaxFreezes)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Plan.GuestPassQuota)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Plan.AccessScope)
            .Must(accessScope => Enumeration.GetAll<AccessScope>().Any(x => x.Id == accessScope))
            .WithMessage("Invalid access scope.");
    }
}