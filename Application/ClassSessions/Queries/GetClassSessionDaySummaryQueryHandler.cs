using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.ClassSessions.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.ClassSessions.Queries;

public sealed class GetClassSessionDaySummaryQueryHandler
    : IRequestHandler<GetClassSessionDaySummaryQuery, Result<ClassSessionDaySummary, Error>>
{
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IReadOnlyRepository<ClassSession, int> _classSessionReadRepository;

    public GetClassSessionDaySummaryQueryHandler(
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IReadOnlyRepository<ClassSession, int> classSessionReadRepository)
    {
        _branchReadRepository = branchReadRepository;
        _classSessionReadRepository = classSessionReadRepository;
    }

    public async Task<Result<ClassSessionDaySummary, Error>> Handle(
        GetClassSessionDaySummaryQuery request,
        CancellationToken cancellationToken)
    {
        // Validate the branch when the schedule is filtered to one branch.
        if (request.BranchId.HasValue)
        {
            bool branchExists = await _branchReadRepository.AnyAsync(
                branch => branch.Id == request.BranchId.Value,
                cancellationToken);

            if (!branchExists)
                return Result.Failure<ClassSessionDaySummary, Error>(Error.EntityNotFound(nameof(Branch), request.BranchId.Value));
        }

        // Filter the sessions for the requested day and optional branch.
        IQueryable<ClassSession> query = _classSessionReadRepository
            .GetAll()
            .Where(classSession =>
                classSession.SessionDate == request.Date &&
                (!request.BranchId.HasValue || classSession.BranchId == request.BranchId.Value));

        // Calculate the entire day summary in SQL.
        var summary = await query
            .GroupBy(classSession => 1)
            .Select(group => new
            {
                TotalBookings = group.Sum(classSession =>
                    classSession.Bookings.Count(booking =>
                        booking.Status == BookingStatus.Booked ||
                        booking.Status == BookingStatus.Attended ||
                        booking.Status == BookingStatus.NoShow)),
                AverageFillRate = group.Average(classSession =>
                    (decimal)classSession.Bookings.Count(booking =>
                        booking.Status == BookingStatus.Booked ||
                        booking.Status == BookingStatus.Attended ||
                        booking.Status == BookingStatus.NoShow) /
                    classSession.CapacityLimit * 100)
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Return zeros when there are no sessions for the selected day.
        ClassSessionDaySummary response = new(
            summary?.TotalBookings ?? 0,
            decimal.Round(summary?.AverageFillRate ?? 0, 2));

        return Result.Success<ClassSessionDaySummary, Error>(response);
    }
}