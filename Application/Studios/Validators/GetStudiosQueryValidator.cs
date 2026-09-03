using FluentValidation;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.Studios.Queries;

public sealed class GetStudiosQueryValidator
    : AbstractValidator<GetStudiosQuery>
{
    public GetStudiosQueryValidator()
    {
        RuleFor(x => x.BranchId).ValidId();
    }
}