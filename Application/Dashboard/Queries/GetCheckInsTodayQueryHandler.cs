using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Dashboard.Contract;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Dashboard.Queries;

public sealed class GetCheckInsTodayQueryHandler
    : IRequestHandler<GetCheckInsTodayQuery, Result<CheckInsTodayDetails, Error>>
{
    private readonly IReadOnlyRepository<CheckIn, int> _checkInReadRepository;

    public GetCheckInsTodayQueryHandler(
        IReadOnlyRepository<CheckIn, int> checkInReadRepository)
    {
        _checkInReadRepository = checkInReadRepository;
    }

    public async Task<Result<CheckInsTodayDetails, Error>> Handle(
        GetCheckInsTodayQuery request,
        CancellationToken cancellationToken)
    {
        DateTime todayStart = DateTime.Today;
        DateTime tomorrowStart = todayStart.AddDays(1);
        DateTime lastWeekStart = todayStart.AddDays(-7);
        DateTime lastWeekEnd = lastWeekStart.AddDays(1);

        // Count today and the same day last week in one query.
        var counts = await _checkInReadRepository
            .GetAll()
            .Where(checkIn =>
                (checkIn.DateTime >= todayStart && checkIn.DateTime < tomorrowStart) ||
                (checkIn.DateTime >= lastWeekStart && checkIn.DateTime < lastWeekEnd))
            .GroupBy(checkIn => 1)
            .Select(group => new
            {
                Today = group.Count(checkIn =>
                    checkIn.DateTime >= todayStart &&
                    checkIn.DateTime < tomorrowStart),
                LastWeek = group.Count(checkIn =>
                    checkIn.DateTime >= lastWeekStart &&
                    checkIn.DateTime < lastWeekEnd)
            })
            .FirstOrDefaultAsync(cancellationToken);

        int checkInsToday = counts?.Today ?? 0;
        int checkInsSameDayLastWeek = counts?.LastWeek ?? 0;

        // Calculate the percentage change shown on the Dashboard.
        decimal percentageChange = checkInsSameDayLastWeek == 0
            ? 0
            : Math.Round(
                (checkInsToday - checkInsSameDayLastWeek) /
                (decimal)checkInsSameDayLastWeek * 100,
                2);

        CheckInsTodayDetails response = new(
            checkInsToday,
            percentageChange);

        return Result.Success<CheckInsTodayDetails, Error>(response);
    }
}