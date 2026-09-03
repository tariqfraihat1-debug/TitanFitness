using FluentValidation;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Application.Plans.Queries;

public sealed class GetPlansQueryValidator : AbstractValidator<GetPlansQuery>
{
    public GetPlansQueryValidator()
    {
        RuleFor(x => x.AccessScope)
            .Must(accessScope => accessScope is null || Enumeration.GetAll<AccessScope>().Any(x => x.Id == accessScope))
            .WithMessage("Invalid access scope.");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Page)
            .GreaterThan(0);
    }
}