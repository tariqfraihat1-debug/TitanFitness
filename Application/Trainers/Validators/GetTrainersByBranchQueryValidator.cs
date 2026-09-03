using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Trainers.Queries;

namespace TitanFitness.Application.Trainers.Validators;

public sealed class GetTrainersByBranchQueryValidator
    : AbstractValidator<GetAvailableTrainersQuery>
{
    public GetTrainersByBranchQueryValidator()
    {
        RuleFor(query => query.BranchId)
            .ValidId();
    }
}