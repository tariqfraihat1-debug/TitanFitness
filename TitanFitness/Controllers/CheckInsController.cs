using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.CheckIns.Commands;
using TitanFitness.Application.CheckIns.Contract;
using TitanFitness.Common.Extensions;
using TitanFitness.Common.Responses;

namespace TitanFitness.Controllers;

[ApiController]
[Route("api/check-ins")]
public sealed class CheckInsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CheckInsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCheckIn(
        CreateCheckInRequest checkInDto,
        CancellationToken cancellationToken)
    {
        CreateCheckInCommand command = new CreateCheckInCommand(checkInDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreated(this);
    }
}