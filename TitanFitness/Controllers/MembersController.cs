using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Members.Commands;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Application.Members.Queries;
using TitanFitness.Common.Extensions;
using TitanFitness.Common.Responses;

namespace TitanFitness.Controllers;

[ApiController]
[Route("api/members")]
public sealed class MembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMembers(
        [FromQuery] int? branchId,
        [FromQuery] string? search,
        [FromQuery] int page,
        CancellationToken cancellationToken)
    {
        GetMembersQuery query = new GetMembersQuery(branchId, search, page);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("{memberId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMemberById(
        int memberId,
        CancellationToken cancellationToken)
    {
        GetMemberByIdQuery query = new GetMemberByIdQuery(memberId);
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
    public async Task<IActionResult> CreateMember(
        CreateMemberRequest memberDto,
        CancellationToken cancellationToken)
    {
        CreateMemberCommand command = new CreateMemberCommand(memberDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreated(this);
    }

    [HttpPut("{memberId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMember(
        int memberId,
        UpdateMemberRequest memberDto,
        CancellationToken cancellationToken)
    {
        UpdateMemberCommand command = new UpdateMemberCommand(memberId, memberDto);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToNoContent(this);
    }

    [HttpGet("{memberId:int}/activity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMemberActivity(
        int memberId,
        CancellationToken cancellationToken)
    {
        GetMemberActivityQuery query = new GetMemberActivityQuery(memberId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("{memberId:int}/current-membership")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentMembership(
        int memberId,
        CancellationToken cancellationToken)
    {
        GetCurrentMembershipQuery query = new GetCurrentMembershipQuery(memberId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }

    [HttpGet("{memberId:int}/entry-eligibility")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEntryEligibility(
        int memberId,
        [FromQuery] int branchId,
        CancellationToken cancellationToken)
    {
        GetEntryEligibilityQuery query = new GetEntryEligibilityQuery(memberId, branchId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToOk(this);
    }
}