using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Plans.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Plans.Queries;

public sealed record GetPlanByIdQuery(
    int PlanId)
    : IRequest<Result<PlanDetails, Error>>;