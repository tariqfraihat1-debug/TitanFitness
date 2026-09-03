using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Memberships.Commands;
using TitanFitness.Application.Memberships.Contract;
using TitanFitness.Application.Memberships.Queries;
using TitanFitness.Common.Extensions;
using TitanFitness.Common.Responses;

namespace TitanFitness.Controllers;

[ApiController]
[Route("api/memberships")]
public sealed class MembershipsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembershipsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{membershipId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMembershipById(
        int membershipId,
        CancellationToken cancellationToken)
    {
        GetMembershipByIdQuery query = new GetMembershipByIdQuery(membershipId);
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
    public async Task<IActionResult> CreateMembership(
    CreateMembershipRequest membershipDto,
    CancellationToken cancellationToken)
    {
        CreateMembershipCommand command = new CreateMembershipCommand(membershipDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreated(this);
    }

    [HttpPost("{membershipId:int}/renew")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RenewMembership(
        int membershipId,
        CancellationToken cancellationToken)
    {
        RenewMembershipCommand command = new RenewMembershipCommand(membershipId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreated(this);
    }

    [HttpGet("{membershipId:int}/available-plans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvailablePlans(
    int membershipId,
    CancellationToken cancellationToken)
    {
        GetAvailablePlansQuery query = new GetAvailablePlansQuery(membershipId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpPost("{membershipId:int}/change-plan")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeMembershipPlan(
        int membershipId,
        ChangeMembershipPlanRequest planDto,
        CancellationToken cancellationToken)
    {
        ChangeMembershipPlanCommand command = new ChangeMembershipPlanCommand(membershipId, planDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreated(this);
    }

    [HttpPost("{membershipId:int}/freezes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddFreeze(
        int membershipId,
        AddFreezeRequest freezeDto,
        CancellationToken cancellationToken)
    {
        AddFreezeCommand command = new AddFreezeCommand(membershipId, freezeDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreated(this);
    }
}