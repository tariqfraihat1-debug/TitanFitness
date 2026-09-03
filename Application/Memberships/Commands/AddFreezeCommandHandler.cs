using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Memberships;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Memberships.Commands;

public sealed class AddFreezeCommandHandler
    : IRequestHandler<AddFreezeCommand, Result<int, Error>>
{
    private readonly IWriteRepository<Membership> _membershipWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddFreezeCommandHandler(
        IWriteRepository<Membership> membershipWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipWriteRepository = membershipWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int, Error>> Handle(
        AddFreezeCommand request,
        CancellationToken cancellationToken)
    {
        // Load the membership with its existing freezes for domain validation.
        Maybe<Membership> membership = await _membershipWriteRepository
            .Query()
            .Include(membership => membership.Freezes)
            .FirstOrDefaultAsync(
                membership => membership.Id == request.MembershipId,
                cancellationToken);

        if (membership.HasNoValue)
            return Result.Failure<int, Error>(
                Error.EntityNotFound(nameof(Membership), request.MembershipId));

        // Validate the selected freeze duration.
        Maybe<FreezeDuration> freezeDuration = Enumeration
            .GetAll<FreezeDuration>()
            .FirstOrDefault(duration =>
                duration.Id == request.Freeze.FreezeDurationId);

        if (freezeDuration.HasNoValue)
            return Result.Failure<int, Error>(
                Error.InvalidValue(
                    nameof(FreezeDuration),
                    request.Freeze.FreezeDurationId));

        // Validate the selected freeze reason.
        Maybe<FreezeReason> freezeReason = Enumeration
            .GetAll<FreezeReason>()
            .FirstOrDefault(reason =>
                reason.Id == request.Freeze.FreezeReasonId);

        if (freezeReason.HasNoValue)
            return Result.Failure<int, Error>(
                Error.InvalidValue(
                    nameof(FreezeReason),
                    request.Freeze.FreezeReasonId));

        DateTime requestedOn = DateTime.Now;
        DateOnly today = DateOnly.FromDateTime(requestedOn);

        // Let the aggregate validate and add the freeze.
        Result<Freeze, Error> freezeResult = membership.Value.AddFreeze(
            request.Freeze.StartDate,
            freezeDuration.Value.Months,
            freezeReason.Value,
            request.Freeze.Notes,
            requestedOn,
            today);

        if (freezeResult.IsFailure)
            return Result.Failure<int, Error>(freezeResult.Error);

        // Persist the new freeze and extended membership end date.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<int, Error>(freezeResult.Value.Id);
    }
}