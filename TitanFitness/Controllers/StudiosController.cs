using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Studios.Queries;
using TitanFitness.Common.Extensions;
using TitanFitness.Common.Responses;

namespace TitanFitness.Controllers;

[ApiController]
[Route("api/studios")]
public sealed class StudiosController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudiosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudios(
        [FromQuery] int branchId,
        CancellationToken cancellationToken)
    {
        GetStudiosQuery query = new GetStudiosQuery(branchId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }
}