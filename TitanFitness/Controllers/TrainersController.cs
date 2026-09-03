using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Trainers.Commands;
using TitanFitness.Application.Trainers.Contract;
using TitanFitness.Application.Trainers.Queries;
using TitanFitness.Common.Extensions;
using TitanFitness.Common.Responses;

namespace TitanFitness.Controllers;

[ApiController]
[Route("api/trainers")]
public sealed class TrainersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrainersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTrainers(
        [FromQuery] int? branchId,
        [FromQuery] string? search,
        [FromQuery] int page,
        CancellationToken cancellationToken)
    {
        GetTrainersQuery query = new GetTrainersQuery(branchId, search, page);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("available")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvailableTrainers(
        [FromQuery] int branchId,
        CancellationToken cancellationToken)
    {
        GetAvailableTrainersQuery query = new GetAvailableTrainersQuery(branchId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("{trainerId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTrainerById(
        int trainerId,
        CancellationToken cancellationToken)
    {
        GetTrainerByIdQuery query = new GetTrainerByIdQuery(trainerId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTrainer(
        CreateTrainerRequest trainerDto,
        CancellationToken cancellationToken)
    {
        CreateTrainerCommand command = new CreateTrainerCommand(trainerDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreated(this);
    }

    [HttpPut("{trainerId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTrainer(
        int trainerId,
        UpdateTrainerRequest trainerDto,
        CancellationToken cancellationToken)
    {
        UpdateTrainerCommand command = new UpdateTrainerCommand(trainerId, trainerDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToNoContent(this);
    }
}