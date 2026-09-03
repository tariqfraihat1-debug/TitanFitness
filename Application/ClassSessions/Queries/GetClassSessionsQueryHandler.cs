using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.ClassSessions.Contract;
using TitanFitness.Application.Common.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.ClassSessions.Queries;

public sealed class GetClassSessionsQueryHandler
    : IRequestHandler<GetClassSessionsQuery, Result<PagedResponse<ClassSessionListItem>, Error>>
{
    private const int PageSize = 3;

    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IReadOnlyRepository<ClassSession, int> _classSessionReadRepository;
    private readonly IReadOnlyRepository<Studio, int> _studioReadRepository;
    private readonly IReadOnlyRepository<Trainer, int> _trainerReadRepository;

    public GetClassSessionsQueryHandler(
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IReadOnlyRepository<ClassSession, int> classSessionReadRepository,
        IReadOnlyRepository<Studio, int> studioReadRepository,
        IReadOnlyRepository<Trainer, int> trainerReadRepository)
    {
        _branchReadRepository = branchReadRepository;
        _classSessionReadRepository = classSessionReadRepository;
        _studioReadRepository = studioReadRepository;
        _trainerReadRepository = trainerReadRepository;
    }

    public async Task<Result<PagedResponse<ClassSessionListItem>, Error>> Handle(
        GetClassSessionsQuery request,
        CancellationToken cancellationToken)
    {
        // Validate the branch when the schedule is filtered to one branch.
        if (request.BranchId.HasValue)
        {
            bool branchExists = await _branchReadRepository.AnyAsync(
                branch => branch.Id == request.BranchId.Value,
                cancellationToken);

            if (!branchExists)
                return Result.Failure<PagedResponse<ClassSessionListItem>, Error>(Error.EntityNotFound(nameof(Branch), request.BranchId.Value));
        }

        // Build the schedule query for the selected date and optional branch.
        var query =
            from classSession in _classSessionReadRepository.GetAll()
            join studio in _studioReadRepository.GetAll()
                on classSession.StudioId equals studio.Id
            join trainer in _trainerReadRepository.GetAll()
                on classSession.TrainerId equals trainer.Id
            where (!request.BranchId.HasValue || classSession.BranchId == request.BranchId.Value) &&
                  classSession.SessionDate == request.Date
            orderby classSession.StartTime
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
            };

        // Get the total number of sessions for pagination.
        int totalCount = await query.CountAsync(cancellationToken);

        // Load only the requested page.
        var sessions = await query
            .Skip((request.Page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);

        DateTime now = DateTime.Now;

        // Build the list using the live session status from the Domain.
        List<ClassSessionListItem> items = sessions
            .Select(session => new ClassSessionListItem(
                session.ClassSession.Id,
                session.ClassSession.ClassName,
                session.StudioName,
                session.TrainerName,
                session.ClassSession.SessionDate,
                session.ClassSession.StartTime,
                session.ClassSession.DurationMinutes,
                session.ClassSession.CapacityLimit,
                session.BookedCount,
                session.WaitlistCount,
                session.ClassSession.GetStatusAt(now).Name))
            .ToList();

        // Build the paged response.
        PagedResponse<ClassSessionListItem> response = new()
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
        };

        return Result.Success<PagedResponse<ClassSessionListItem>, Error>(response);
    }
}