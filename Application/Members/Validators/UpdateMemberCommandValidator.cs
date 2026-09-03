using FluentValidation;
using TitanFitness.Application.Common.Validators;
using TitanFitness.Application.Members.Commands;

namespace TitanFitness.Application.Members.Validators;

public sealed class UpdateMemberCommandValidator
    : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(command => command.MemberId)
            .ValidId();

        RuleFor(command => command.Member.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Member.Email)
            .MaximumLength(100)
            .When(command => !string.IsNullOrWhiteSpace(command.Member.Email));

        RuleFor(command => command.Member.Phone)
            .MaximumLength(20)
            .When(command => !string.IsNullOrWhiteSpace(command.Member.Phone));

        RuleFor(command => command.Member.Address)
            .MaximumLength(200)
            .When(command => !string.IsNullOrWhiteSpace(command.Member.Address));

        RuleFor(command => command.Member.JoinedDate)
            .NotEmpty();

        RuleFor(command => command.Member.HomeBranchId)
            .ValidId();
    }
}