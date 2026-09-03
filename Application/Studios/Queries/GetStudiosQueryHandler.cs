using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Studios.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Studios.Queries;

public sealed class GetStudiosQueryHandler
    : IRequestHandler<GetStudiosQuery, Result<IReadOnlyList<StudioResponse>, Error>>
{
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IReadOnlyRepository<Studio, int> _studioReadRepository;

    public GetStudiosQueryHandler(
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IReadOnlyRepository<Studio, int> studioReadRepository)
    {
        _branchReadRepository = branchReadRepository;
        _studioReadRepository = studioReadRepository;
    }

    public async Task<Result<IReadOnlyList<StudioResponse>, Error>> Handle(
        GetStudiosQuery request,
        CancellationToken cancellationToken)
    {
        // Validate the selected branch.
        bool branchExists = await _branchReadRepository
            .GetAll()
            .AnyAsync(branch => branch.Id == request.BranchId, cancellationToken);

        if (!branchExists)
            return Result.Failure<IReadOnlyList<StudioResponse>, Error>(Error.EntityNotFound(nameof(Branch), request.BranchId));

        // Load the studios available in the selected branch.
        IReadOnlyList<StudioResponse> studios = await _studioReadRepository
            .GetAll()
            .Where(studio => studio.BranchId == request.BranchId)
            .OrderBy(studio => studio.Name)
            .Select(studio => new StudioResponse(
                studio.Id,
                studio.Name,
                studio.Capacity))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<StudioResponse>, Error>(studios);
    }
}