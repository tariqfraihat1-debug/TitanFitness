using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.ClassSessions.Commands;

public sealed class CreateClassSessionCommandHandler
    : IRequestHandler<CreateClassSessionCommand, Result<int, Error>>
{
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IReadOnlyRepository<Studio, int> _studioReadRepository;
    private readonly IReadOnlyRepository<Trainer, int> _trainerReadRepository;
    private readonly IReadOnlyRepository<ClassSession, int> _classSessionReadRepository;
    private readonly IWriteRepository<ClassSession> _classSessionWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateClassSessionCommandHandler(
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IReadOnlyRepository<Studio, int> studioReadRepository,
        IReadOnlyRepository<Trainer, int> trainerReadRepository,
        IReadOnlyRepository<ClassSession, int> classSessionReadRepository,
        IWriteRepository<ClassSession> classSessionWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository;
        _studioReadRepository = studioReadRepository;
        _trainerReadRepository = trainerReadRepository;
        _classSessionReadRepository = classSessionReadRepository;
        _classSessionWriteRepository = classSessionWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int, Error>> Handle(
        CreateClassSessionCommand request,
        CancellationToken cancellationToken)
    {
        // Validate branch, studio and trainer combination in one query.
        var setup = await (
            from branch in _branchReadRepository.GetAll()
            join studio in _studioReadRepository.GetAll()
                on branch.Id equals studio.BranchId
            join trainer in _trainerReadRepository.GetAll()
                on branch.Id equals trainer.BranchId
            where branch.Id == request.ClassSession.BranchId &&
                  studio.Id == request.ClassSession.StudioId &&
                  trainer.Id == request.ClassSession.TrainerId
            select new
            {
                StudioCapacity = studio.Capacity,
                TrainerIsActive = trainer.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Return the specific setup error when the combination is invalid.
        if (setup is null)
        {
            bool branchExists = await _branchReadRepository.AnyAsync(
                branch => branch.Id == request.ClassSession.BranchId,
                cancellationToken);

            if (!branchExists)
                return Result.Failure<int, Error>(
                    Error.EntityNotFound(nameof(Branch), request.ClassSession.BranchId));

            Maybe<Studio> studio = await _studioReadRepository.FirstOrDefaultAsync(
                studio => studio.Id == request.ClassSession.StudioId,
                cancellationToken);

            if (studio.HasNoValue)
                return Result.Failure<int, Error>(
                    Error.EntityNotFound(nameof(Studio), request.ClassSession.StudioId));

            if (studio.Value.BranchId != request.ClassSession.BranchId)
                return Result.Failure<int, Error>(
                    Error.InvalidOperation("The selected studio does not belong to the selected branch."));

            Maybe<Trainer> trainer = await _trainerReadRepository.FirstOrDefaultAsync(
                trainer => trainer.Id == request.ClassSession.TrainerId,
                cancellationToken);

            if (trainer.HasNoValue)
                return Result.Failure<int, Error>(
                    Error.EntityNotFound(nameof(Trainer), request.ClassSession.TrainerId));

            return Result.Failure<int, Error>(
                Error.InvalidOperation("The selected trainer does not belong to the selected branch."));
        }

        // Only active trainers can be scheduled.
        if (!setup.TrainerIsActive)
            return Result.Failure<int, Error>(Error.InvalidOperation("Only an active trainer can be scheduled."));

        // Session capacity cannot exceed studio capacity.
        if (request.ClassSession.CapacityLimit > setup.StudioCapacity)
            return Result.Failure<int, Error>(Error.InvalidOperation("Capacity limit cannot exceed the selected studio capacity."));

        // Load same-day sessions that may conflict with the trainer or studio.
        var existingSessions = await _classSessionReadRepository
            .GetAll()
            .Where(classSession =>
                classSession.SessionDate == request.ClassSession.SessionDate &&
                classSession.Status != ClassSessionStatus.Cancelled &&
                (classSession.TrainerId == request.ClassSession.TrainerId ||
                 classSession.StudioId == request.ClassSession.StudioId))
            .Select(classSession => new
            {
                classSession.TrainerId,
                classSession.StudioId,
                classSession.StartTime,
                classSession.DurationMinutes
            })
            .ToListAsync(cancellationToken);

        DateTime newSessionStart = request.ClassSession.SessionDate
            .ToDateTime(request.ClassSession.StartTime);

        DateTime newSessionEnd = newSessionStart
            .AddMinutes(request.ClassSession.DurationMinutes);

        // Prevent the trainer from running overlapping sessions.
        bool trainerOverlaps = existingSessions.Any(classSession =>
        {
            DateTime existingStart = request.ClassSession.SessionDate
                .ToDateTime(classSession.StartTime);

            DateTime existingEnd = existingStart
                .AddMinutes(classSession.DurationMinutes);

            return classSession.TrainerId == request.ClassSession.TrainerId &&
                   existingStart < newSessionEnd &&
                   existingEnd > newSessionStart;
        });

        if (trainerOverlaps)
            return Result.Failure<int, Error>(
                Error.InvalidOperation("The trainer is already scheduled for an overlapping session."));

        // Prevent the studio from hosting overlapping sessions.
        bool studioOverlaps = existingSessions.Any(classSession =>
        {
            DateTime existingStart = request.ClassSession.SessionDate
                .ToDateTime(classSession.StartTime);

            DateTime existingEnd = existingStart
                .AddMinutes(classSession.DurationMinutes);

            return classSession.StudioId == request.ClassSession.StudioId &&
                   existingStart < newSessionEnd &&
                   existingEnd > newSessionStart;
        });

        if (studioOverlaps)
            return Result.Failure<int, Error>(Error.InvalidOperation("The studio is already scheduled for an overlapping session."));

        DateTime now = DateTime.Now;

        // Let the aggregate validate and create the new class session.
        Result<ClassSession, Error> classSessionResult = ClassSession.Create(
            request.ClassSession.ClassName,
            request.ClassSession.BranchId,
            request.ClassSession.StudioId,
            request.ClassSession.TrainerId,
            request.ClassSession.SessionDate,
            request.ClassSession.StartTime,
            request.ClassSession.DurationMinutes,
            request.ClassSession.CapacityLimit,
            request.ClassSession.Description,
            now);

        if (classSessionResult.IsFailure)
            return Result.Failure<int, Error>(classSessionResult.Error);

        // Persist the new session.
        await _classSessionWriteRepository.AddAsync(
            classSessionResult.Value,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<int, Error>(classSessionResult.Value.Id);
    }
}