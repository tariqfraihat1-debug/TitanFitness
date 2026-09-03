using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Common.Contract;
using TitanFitness.Application.Trainers.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Trainers.Queries;

public sealed class GetTrainersQueryHandler
    : IRequestHandler<GetTrainersQuery, Result<PagedResponse<TrainerListItem>, Error>>
{
    private const int PageSize = 4;

    private readonly IReadOnlyRepository<Trainer, int> _trainerReadRepository;
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;

    public GetTrainersQueryHandler(
        IReadOnlyRepository<Trainer, int> trainerReadRepository,
        IReadOnlyRepository<Branch, int> branchReadRepository)
    {
        _trainerReadRepository = trainerReadRepository;
        _branchReadRepository = branchReadRepository;
    }

    public async Task<Result<PagedResponse<TrainerListItem>, Error>> Handle(
        GetTrainersQuery request,
        CancellationToken cancellationToken)
    {
        // Validate the selected branch when filtering.
        if (request.BranchId.HasValue)
        {
            bool branchExists = await _branchReadRepository.AnyAsync(
                branch => branch.Id == request.BranchId.Value,
                cancellationToken);

            if (!branchExists)
                return Result.Failure<PagedResponse<TrainerListItem>, Error>(
                    Error.EntityNotFound(nameof(Branch), request.BranchId.Value));
        }

        string? search = string.IsNullOrWhiteSpace(request.Search)
            ? null
            : request.Search.Trim();

        bool searchIsId = int.TryParse(search, out int trainerId);

        bool activeMatches = search != null &&
            search.Equals("Active", StringComparison.OrdinalIgnoreCase);

        bool inactiveMatches = search != null &&
            search.Equals("Inactive", StringComparison.OrdinalIgnoreCase);

        // Build the directory with branch filtering and search.
        var query =
            from trainer in _trainerReadRepository.GetAll()
            join branch in _branchReadRepository.GetAll()
                on trainer.BranchId equals branch.Id
            where
                (!request.BranchId.HasValue ||
                 trainer.BranchId == request.BranchId.Value) &&
                (search == null ||
                 trainer.Name.Contains(search) ||
                 (searchIsId && trainer.Id == trainerId) ||
                 (trainer.Specialty != null && trainer.Specialty.Contains(search)) ||
                 branch.Name.Contains(search) ||
                 (activeMatches && trainer.IsActive) ||
                 (inactiveMatches && !trainer.IsActive))
            select new
            {
                trainer.Id,
                trainer.Name,
                trainer.Specialty,
                BranchName = branch.Name,
                trainer.IsActive
            };

        // Get the total count for pagination.
        int totalCount = await query.CountAsync(cancellationToken);

        int page = request.Page < 1 ? 1 : request.Page;

        // Load the requested directory page.
        List<TrainerListItem> trainers = await query
            .OrderBy(trainer => trainer.Name)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(trainer => new TrainerListItem(
                trainer.Id,
                trainer.Name,
                trainer.Specialty,
                trainer.BranchName,
                trainer.IsActive))
            .ToListAsync(cancellationToken);

        // Build the paged response.
        PagedResponse<TrainerListItem> response = new()
        {
            Items = trainers,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
        };

        return Result.Success<PagedResponse<TrainerListItem>, Error>(response);
    }
}