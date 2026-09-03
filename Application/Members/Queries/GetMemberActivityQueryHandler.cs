using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Members.Queries;

public sealed class GetMemberActivityQueryHandler
    : IRequestHandler<GetMemberActivityQuery, Result<List<MemberActivity>, Error>>
{
    private const int ActivityCount = 7;

    private readonly IReadOnlyRepository<Member, int> _memberReadRepository;
    private readonly IReadOnlyRepository<CheckIn, int> _checkInReadRepository;
    private readonly IReadOnlyRepository<ClassSession, int> _classSessionReadRepository;
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IReadOnlyRepository<Trainer, int> _trainerReadRepository;

    public GetMemberActivityQueryHandler(
        IReadOnlyRepository<Member, int> memberReadRepository,
        IReadOnlyRepository<CheckIn, int> checkInReadRepository,
        IReadOnlyRepository<ClassSession, int> classSessionReadRepository,
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IReadOnlyRepository<Trainer, int> trainerReadRepository)
    {
        _memberReadRepository = memberReadRepository;
        _checkInReadRepository = checkInReadRepository;
        _classSessionReadRepository = classSessionReadRepository;
        _branchReadRepository = branchReadRepository;
        _trainerReadRepository = trainerReadRepository;
    }

    public async Task<Result<List<MemberActivity>, Error>> Handle(
        GetMemberActivityQuery request,
        CancellationToken cancellationToken)
    {
        // Load the member's last seven check-ins, including admitted and refused attempts.
        List<MemberActivity> checkInActivities = await (
            from checkIn in _checkInReadRepository.GetAll()
            join branch in _branchReadRepository.GetAll()
                on checkIn.BranchId equals branch.Id
            where checkIn.MemberId == request.MemberId
            orderby checkIn.DateTime descending
            select new MemberActivity(
                "Facility Check-in",
                branch.Name,
                checkIn.DateTime,
                checkIn.CheckInResult.Name,
                checkIn.RefusalReason))
            .Take(ActivityCount)
            .ToListAsync(cancellationToken);

        // Load the member's last seven attended classes.
        var attendedClasses = await (
            from classSession in _classSessionReadRepository.GetAll()
            join trainer in _trainerReadRepository.GetAll()
                on classSession.TrainerId equals trainer.Id
            from booking in classSession.Bookings
            where booking.MemberId == request.MemberId &&
                  booking.Status == BookingStatus.Attended
            orderby classSession.SessionDate descending,
                    classSession.StartTime descending
            select new
            {
                classSession.ClassName,
                TrainerName = trainer.Name,
                classSession.SessionDate,
                classSession.StartTime
            })
            .Take(ActivityCount)
            .ToListAsync(cancellationToken);

        // Convert class dates after materialization.
        List<MemberActivity> classActivities = attendedClasses
            .Select(activity => new MemberActivity(
                "Class Attendance",
                $"{activity.ClassName} with {activity.TrainerName}",
                activity.SessionDate.ToDateTime(activity.StartTime),
                null,
                null))
            .ToList();

        // Validate the member when no activity exists.
        if (checkInActivities.Count == 0 && classActivities.Count == 0)
        {
            bool memberExists = await _memberReadRepository.AnyAsync(
                member => member.Id == request.MemberId,
                cancellationToken);

            if (!memberExists)
                return Result.Failure<List<MemberActivity>, Error>(
                    Error.EntityNotFound(nameof(Member), request.MemberId));
        }

        // Merge both activity types and return the most recent seven.
        List<MemberActivity> activities = checkInActivities
            .Concat(classActivities)
            .OrderByDescending(activity => activity.DateTime)
            .Take(ActivityCount)
            .ToList();

        return Result.Success<List<MemberActivity>, Error>(activities);
    }
}