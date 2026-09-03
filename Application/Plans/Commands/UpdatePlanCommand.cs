using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Plans.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Plans.Commands;

public sealed record UpdatePlanCommand(
    int PlanId,
    UpdatePlanRequest Plan)
    : IRequest<UnitResult<Error>>;