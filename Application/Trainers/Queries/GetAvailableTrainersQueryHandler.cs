using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Trainers.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Trainers.Queries;

public sealed class GetAvailableTrainersQueryHandler
    : IRequestHandler<GetAvailableTrainersQuery, Result<List<TrainerLookupItem>, Error>>
{
    private readonly IReadOnlyRepository<Trainer, int> _trainerReadRepository;
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;

    public GetAvailableTrainersQueryHandler(
        IReadOnlyRepository<Trainer, int> trainerReadRepository,
        IReadOnlyRepository<Branch, int> branchReadRepository)
    {
        _trainerReadRepository = trainerReadRepository;
        _branchReadRepository = branchReadRepository;
    }

    public async Task<Result<List<TrainerLookupItem>, Error>> Handle(
        GetAvailableTrainersQuery request,
        CancellationToken cancellationToken)
    {
        // Validate the selected branch.
        bool branchExists = await _branchReadRepository.AnyAsync(
            branch => branch.Id == request.BranchId,
            cancellationToken);

        if (!branchExists)
            return Result.Failure<List<TrainerLookupItem>, Error>(
                Error.EntityNotFound(nameof(Branch), request.BranchId));

        // Load active trainers from the selected branch for class scheduling.
        List<TrainerLookupItem> trainers = await _trainerReadRepository
            .GetAll()
            .Where(trainer =>
                trainer.BranchId == request.BranchId &&
                trainer.IsActive)
            .OrderBy(trainer => trainer.Name)
            .Select(trainer => new TrainerLookupItem(
                trainer.Id,
                trainer.Name,
                trainer.Specialty))
            .ToListAsync(cancellationToken);

        return Result.Success<List<TrainerLookupItem>, Error>(trainers);
    }
}