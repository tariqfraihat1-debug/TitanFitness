using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Branches.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Branches.Queries;

public sealed class GetBranchesQueryHandler
    : IRequestHandler<GetBranchesQuery, Result<IReadOnlyList<BranchListItem>, Error>>
{
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;

    public GetBranchesQueryHandler(
        IReadOnlyRepository<Branch, int> branchReadRepository)
    {
        _branchReadRepository = branchReadRepository;
    }

    public async Task<Result<IReadOnlyList<BranchListItem>, Error>> Handle(
        GetBranchesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BranchListItem> branches =
            await _branchReadRepository
                .GetAll()
                .OrderBy(branch => branch.Name)
                .Select(branch => new BranchListItem(
                    branch.Id,
                    branch.Name))
                .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<BranchListItem>, Error>(
            branches);
    }
}