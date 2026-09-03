using FluentValidation;
using TitanFitness.Application.ClassSessions.Queries;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.ClassSessions.Validators;

public sealed class GetClassSessionDaySummaryQueryValidator
    : AbstractValidator<GetClassSessionDaySummaryQuery>
{
    public GetClassSessionDaySummaryQueryValidator()
    {
        RuleFor(query => query.BranchId!.Value)
            .ValidId()
            .When(query => query.BranchId.HasValue);

        RuleFor(query => query.Date)
            .NotEmpty();
    }
}