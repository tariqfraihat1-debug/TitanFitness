using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Plans.Commands;

public sealed class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, UnitResult<Error>>
{
    private readonly IWriteRepository<Plan> _planWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePlanCommandHandler(
        IWriteRepository<Plan> planWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _planWriteRepository = planWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<Error>> Handle(
        UpdatePlanCommand request,
        CancellationToken cancellationToken)
    {
        // Load the tracked plan.
        Maybe<Plan> plan = await _planWriteRepository.FindAsync(
            [request.PlanId],
            cancellationToken);

        if (plan.HasNoValue)
            return UnitResult.Failure(Error.EntityNotFound(nameof(Plan), request.PlanId));

        // Validate the selected access scope.
        Maybe<AccessScope> accessScope = Enumeration.GetAll<AccessScope>()
            .FirstOrDefault(scope => scope.Id == request.Plan.AccessScope);

        if (accessScope.HasNoValue)
            return UnitResult.Failure(Error.InvalidValue(nameof(AccessScope), request.Plan.AccessScope));

        // Update only the Plan; existing membership terms stay unchanged.
        UnitResult<Error> updateResult = plan.Value.Update(
            request.Plan.PlanName,
            request.Plan.Price,
            request.Plan.DurationInMonths,
            request.Plan.MaxFreezeDays,
            request.Plan.MaxFreezes,
            request.Plan.GuestPassQuota,
            accessScope.Value,
            request.Plan.IsPublished);

        if (updateResult.IsFailure)
            return updateResult;

        // Persist the Plan changes.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}