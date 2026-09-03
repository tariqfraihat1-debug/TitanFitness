using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Studios.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Studios.Queries;

public sealed record GetStudiosQuery(
    int BranchId)
    : IRequest<Result<IReadOnlyList<StudioResponse>, Error>>;