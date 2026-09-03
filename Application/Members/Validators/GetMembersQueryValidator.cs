using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Members.Queries;

namespace TitanFitness.Application.Members.Validators;

public sealed class GetMembersQueryValidator
    : AbstractValidator<GetMembersQuery>
{
    public GetMembersQueryValidator()
    {
        RuleFor(query => query.BranchId!.Value)
            .ValidId()
            .When(query => query.BranchId.HasValue);

        RuleFor(query => query.Page)
            .GreaterThan(0);
    }
}