using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.ClassSessions.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.ClassSessions.Queries;

public sealed class GetClassSessionByIdQueryHandler
    : IRequestHandler<GetClassSessionByIdQuery, Result<ClassSessionDetails, Error>>
{
    private readonly IReadOnlyRepository<ClassSession, int> _classSessionReadRepository;
    private readonly IReadOnlyRepository<Studio, int> _studioReadRepository;
    private readonly IReadOnlyRepository<Trainer, int> _trainerReadRepository;

    public GetClassSessionByIdQueryHandler(
        IReadOnlyRepository<ClassSession, int> classSessionReadRepository,
        IReadOnlyRepository<Studio, int> studioReadRepository,
        IReadOnlyRepository<Trainer, int> trainerReadRepository)
    {
        _classSessionReadRepository = classSessionReadRepository;
        _studioReadRepository = studioReadRepository;
        _trainerReadRepository = trainerReadRepository;
    }

    public async Task<Result<ClassSessionDetails, Error>> Handle(
        GetClassSessionByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Load the session, studio, trainer and booking counts in one query.
        var data = await (
            from classSession in _classSessionReadRepository.GetAll()
            join studio in _studioReadRepository.GetAll()
                on classSession.StudioId equals studio.Id
            join trainer in _trainerReadRepository.GetAll()
                on classSession.TrainerId equals trainer.Id
            where classSession.Id == request.SessionId
            select new
            {
                ClassSession = classSession,
                StudioName = studio.Name,
                TrainerName = trainer.Name,
                BookedCount = classSession.Bookings.Count(booking =>
                    booking.Status == BookingStatus.Booked ||
                    booking.Status == BookingStatus.Attended ||
                    booking.Status == BookingStatus.NoShow),
                WaitlistCount = classSession.Bookings.Count(booking =>
                    booking.Status == BookingStatus.Waitlisted)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
            return Result.Failure<ClassSessionDetails, Error>(Error.EntityNotFound(nameof(ClassSession), request.SessionId));

        // Calculate remaining capacity and the live session status.
        int remainingPlaces = Math.Max(data.ClassSession.CapacityLimit - data.BookedCount, 0);
        ClassSessionStatus currentStatus = data.ClassSession.GetStatusAt(DateTime.Now);

        // Build the session details response.
        ClassSessionDetails response = new(
            data.ClassSession.Id,
            data.ClassSession.ClassName,
            data.ClassSession.BranchId,
            data.ClassSession.StudioId,
            data.StudioName,
            data.ClassSession.TrainerId,
            data.TrainerName,
            data.ClassSession.SessionDate,
            data.ClassSession.StartTime,
            data.ClassSession.DurationMinutes,
            data.ClassSession.CapacityLimit,
            data.BookedCount,
            data.WaitlistCount,
            remainingPlaces,
            currentStatus.Name,
            data.ClassSession.Description);

        return Result.Success<ClassSessionDetails, Error>(response);
    }
}