using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Application.CheckIns.Contract;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.CheckIns.Commands;

public sealed record CreateCheckInCommand(
    CreateCheckInRequest CheckIn)
    : IRequest<Result<CheckInDetails, Error>>;