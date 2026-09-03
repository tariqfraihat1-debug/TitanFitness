using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Dashboard.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Dashboard.Queries;

public sealed class GetUpcomingClassesQueryHandler
    : IRequestHandler<GetUpcomingClassesQuery, Result<List<UpcomingClassItem>, Error>>
{
    private readonly IReadOnlyRepository<ClassSession, int> _classSessionReadRepository;
    private readonly IReadOnlyRepository<Studio, int> _studioReadRepository;
    private readonly IReadOnlyRepository<Trainer, int> _trainerReadRepository;

    public GetUpcomingClassesQueryHandler(
        IReadOnlyRepository<ClassSession, int> classSessionReadRepository,
        IReadOnlyRepository<Studio, int> studioReadRepository,
        IReadOnlyRepository<Trainer, int> trainerReadRepository)
    {
        _classSessionReadRepository = classSessionReadRepository;
        _studioReadRepository = studioReadRepository;
        _trainerReadRepository = trainerReadRepository;
    }

    public async Task<Result<List<UpcomingClassItem>, Error>> Handle(
        GetUpcomingClassesQuery request,
        CancellationToken cancellationToken)
    {
        DateTime now = DateTime.Now;
        DateOnly today = DateOnly.FromDateTime(now);
        TimeOnly currentTime = TimeOnly.FromDateTime(now);
        TimeOnly thirtyMinutesAgo = currentTime.AddMinutes(-30);
        TimeOnly fortyFiveMinutesAgo = currentTime.AddMinutes(-45);
        TimeOnly sixtyMinutesAgo = currentTime.AddMinutes(-60);

        // Load only the next two running or upcoming sessions in one query.
        var sessions = await (
            from classSession in _classSessionReadRepository.GetAll()
            join studio in _studioReadRepository.GetAll()
                on classSession.StudioId equals studio.Id
            join trainer in _trainerReadRepository.GetAll()
                on classSession.TrainerId equals trainer.Id
            where classSession.Status != ClassSessionStatus.Cancelled &&
                  (
                      classSession.SessionDate > today ||
                      (
                          classSession.SessionDate == today &&
                          (
                              (classSession.DurationMinutes == 30 && classSession.StartTime > thirtyMinutesAgo) ||
                              (classSession.DurationMinutes == 45 && classSession.StartTime > fortyFiveMinutesAgo) ||
                              (classSession.DurationMinutes == 60 && classSession.StartTime > sixtyMinutesAgo)
                          )
                      )
                  )
            orderby classSession.SessionDate, classSession.StartTime
            select new
            {
                ClassSession = classSession,
                StudioName = studio.Name,
                TrainerName = trainer.Name,
                BookedCount = classSession.Bookings.Count(booking =>
                    booking.Status == BookingStatus.Booked ||
                    booking.Status == BookingStatus.Attended ||
                    booking.Status == BookingStatus.NoShow)
            })
            .Take(2)
            .ToListAsync(cancellationToken);

        // Build the Dashboard items using the live session status.
        List<UpcomingClassItem> response = sessions
            .Select(session => new UpcomingClassItem(
                session.ClassSession.Id,
                session.ClassSession.ClassName,
                session.ClassSession.SessionDate,
                session.ClassSession.StartTime,
                session.StudioName,
                session.TrainerName,
                session.BookedCount,
                session.ClassSession.CapacityLimit,
                session.ClassSession.GetStatusAt(now) == ClassSessionStatus.InProgress
                    ? "In Progress"
                    : "Upcoming"))
            .ToList();

        return Result.Success<List<UpcomingClassItem>, Error>(response);
    }
}