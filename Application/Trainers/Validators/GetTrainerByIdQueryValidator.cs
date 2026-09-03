using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Trainers.Queries;

namespace TitanFitness.Application.Trainers.Validators;

public sealed class GetTrainerByIdQueryValidator
    : AbstractValidator<GetTrainerByIdQuery>
{
    public GetTrainerByIdQueryValidator()
    {
        RuleFor(query => query.TrainerId)
            .ValidId();
    }
}