using FluentValidation;
using TitanFitness.Domain.Common.ValueObjects;

namespace TitanFitness.Application.Common.Validators;

public static class IdValidatorExtensions
{
    public static IRuleBuilderOptions<T, int> ValidId<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .Must(id => Id.Create(id).IsSuccess)
            .WithMessage("{PropertyName} must be greater than zero.");
    }
}