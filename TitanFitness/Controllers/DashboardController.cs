using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Dashboard.Queries;
using TitanFitness.Common.Extensions;
using TitanFitness.Common.Responses;

namespace TitanFitness.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("check-ins-today")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCheckInsToday(CancellationToken cancellationToken)
    {
        GetCheckInsTodayQuery query = new GetCheckInsTodayQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("active-members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveMembers(CancellationToken cancellationToken)
    {
        GetActiveMembersQuery query = new GetActiveMembersQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("upcoming-classes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUpcomingClasses(CancellationToken cancellationToken)
    {
        GetUpcomingClassesQuery query = new GetUpcomingClassesQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }
}