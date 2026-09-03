using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.ClassSessions.Commands;
using TitanFitness.Application.ClassSessions.Contract;
using TitanFitness.Application.ClassSessions.Queries;
using TitanFitness.Common.Extensions;
using TitanFitness.Common.Responses;

namespace TitanFitness.Controllers;

[ApiController]
[Route("api/class-sessions")]
public sealed class ClassSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClassSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetClassSessions(
        [FromQuery] int? branchId,
        [FromQuery] DateOnly date,
        [FromQuery] int page,
        CancellationToken cancellationToken)
    {
        GetClassSessionsQuery query = new GetClassSessionsQuery(branchId, date, page);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("day-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetClassSessionsDaySummary(
        [FromQuery] int? branchId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        GetClassSessionDaySummaryQuery query = new GetClassSessionDaySummaryQuery(branchId, date);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("{sessionId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetClassSessionById(
        int sessionId,
        CancellationToken cancellationToken)
    {
        GetClassSessionByIdQuery query = new GetClassSessionByIdQuery(sessionId);
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
    public async Task<IActionResult> CreateClassSession(
        CreateClassSessionRequest sessionDto,
        CancellationToken cancellationToken)
    {
        CreateClassSessionCommand command = new CreateClassSessionCommand(sessionDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreated(this);
    }
}