using FluentValidation;
using TitanFitness.Application.ClassSessions.Queries;
using TitanFitness.Application.Common.Validators;

namespace TitanFitness.Application.ClassSessions.Validators;

public sealed class GetClassSessionByIdQueryValidator
    : AbstractValidator<GetClassSessionByIdQuery>
{
    public GetClassSessionByIdQueryValidator()
    {
        RuleFor(query => query.SessionId)
            .ValidId();
    }
}