using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Trainers.Queries;

namespace TitanFitness.Application.Trainers.Validators;

public sealed class GetTrainersQueryValidator
    : AbstractValidator<GetTrainersQuery>
{
    public GetTrainersQueryValidator()
    {
        RuleFor(query => query.BranchId!.Value)
            .ValidId()
            .When(query => query.BranchId.HasValue);

        RuleFor(query => query.Search)
            .MaximumLength(100)
            .When(query => !string.IsNullOrWhiteSpace(query.Search));

        RuleFor(query => query.Page)
            .GreaterThan(0);
    }
}