using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.Dashboard.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Dashboard.Queries;

public sealed record GetCheckInsTodayQuery()
    : IRequest<Result<CheckInsTodayDetails, Error>>;