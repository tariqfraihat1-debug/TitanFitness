using FluentValidation;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.Plans.Queries;

public sealed class GetPlanByIdQueryValidator : AbstractValidator<GetPlanByIdQuery>
{
    public GetPlanByIdQueryValidator()
    {
        RuleFor(x => x.PlanId)
            .ValidId();
    }
}