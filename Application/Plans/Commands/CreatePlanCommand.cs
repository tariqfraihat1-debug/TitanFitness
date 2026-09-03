using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Plans.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Plans.Commands;

public sealed record CreatePlanCommand(
    CreatePlanRequest Plan)
    : IRequest<Result<int, Error>>;