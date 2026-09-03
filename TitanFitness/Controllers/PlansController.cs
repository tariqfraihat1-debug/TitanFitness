using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Plans.Commands;
using TitanFitness.Application.Plans.Contract;
using TitanFitness.Application.Plans.Queries;
using TitanFitness.Common.Extensions;
using TitanFitness.Common.Responses;

namespace TitanFitness.Controllers;

[ApiController]
[Route("api/plans")]
public sealed class PlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPlans(
    [FromQuery] AccessScopeFilter? accessScope,
    [FromQuery] string? search,
    [FromQuery] int page,
    CancellationToken cancellationToken)
    {
        GetPlansQuery query = new GetPlansQuery(
            accessScope.HasValue ? (int)accessScope.Value : null,
            search,
            page);

        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("{planId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPlanById(
        int planId,
        CancellationToken cancellationToken)
    {
        GetPlanByIdQuery query = new GetPlanByIdQuery(planId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePlan(
        CreatePlanRequest planDto,
        CancellationToken cancellationToken)
    {
        CreatePlanCommand command = new CreatePlanCommand(planDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreated(this);
    }

    [HttpPut("{planId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePlan(
        int planId,
        UpdatePlanRequest planDto,
        CancellationToken cancellationToken)
    {
        UpdatePlanCommand command = new UpdatePlanCommand(planId, planDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToNoContent(this);
    }
}