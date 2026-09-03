using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Plans.Commands;

public sealed class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Result<int, Error>>
{
    private readonly IWriteRepository<Plan> _planWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePlanCommandHandler(
        IWriteRepository<Plan> planWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _planWriteRepository = planWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int, Error>> Handle(
        CreatePlanCommand request,
        CancellationToken cancellationToken)
    {
        // Validate the selected access scope.
        Maybe<AccessScope> accessScope = Enumeration.GetAll<AccessScope>()
            .FirstOrDefault(scope => scope.Id == request.Plan.AccessScope);

        if (accessScope.HasNoValue)
            return Result.Failure<int, Error>(Error.InvalidValue(nameof(AccessScope), request.Plan.AccessScope));

        // Let the Domain validate and create the plan.
        Result<Plan, Error> planResult = Plan.Create(
            request.Plan.PlanName,
            request.Plan.Price,
            request.Plan.DurationInMonths,
            request.Plan.MaxFreezeDays,
            request.Plan.MaxFreezes,
            request.Plan.GuestPassQuota,
            accessScope.Value,
            request.Plan.IsPublished);

        if (planResult.IsFailure)
            return Result.Failure<int, Error>(planResult.Error);

        // Persist the new plan.
        await _planWriteRepository.AddAsync(planResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<int, Error>(planResult.Value.Id);
    }
}