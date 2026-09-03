using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Common.Contract;
using TitanFitness.Application.Plans.Contract;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Plans.Queries;

public sealed class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, Result<PagedResponse<PlanListItem>, Error>>
{
    private const int PageSize = 4 ;
    private readonly IReadOnlyRepository<Plan, int> _planReadRepository;

    public GetPlansQueryHandler(IReadOnlyRepository<Plan, int> planReadRepository)
    {
        _planReadRepository = planReadRepository;
    }

    public async Task<Result<PagedResponse<PlanListItem>, Error>> Handle(
        GetPlansQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Plan> query = _planReadRepository.GetAll();

        // Filter by access scope when supplied.
        if (request.AccessScope.HasValue)
        {
            Maybe<AccessScope> accessScope = Enumeration.GetAll<AccessScope>()
                .FirstOrDefault(scope => scope.Id == request.AccessScope.Value);

            if (accessScope.HasNoValue)
                return Result.Failure<PagedResponse<PlanListItem>, Error>(Error.InvalidValue(nameof(AccessScope), request.AccessScope.Value));

            query = query.Where(plan => plan.AccessScope == accessScope.Value);
        }

        // Search every value shown in the Plan Catalogue.
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search.Trim();

            bool searchIsDecimal = decimal.TryParse(search, out decimal decimalValue);
            bool searchIsInteger = int.TryParse(search, out int integerValue);

            bool homeBranchOnlyMatches = AccessScope.HomeBranchOnly.Name.Contains(search, StringComparison.OrdinalIgnoreCase);
            bool allBranchesMatches = AccessScope.AllBranches.Name.Contains(search, StringComparison.OrdinalIgnoreCase);
            bool publishedMatches = "Published".Contains(search, StringComparison.OrdinalIgnoreCase);
            bool unpublishedMatches = "Unpublished".Contains(search, StringComparison.OrdinalIgnoreCase);

            query = query.Where(plan =>
                plan.PlanName.Contains(search) ||

                (searchIsDecimal && plan.Price == decimalValue) ||

                (searchIsInteger && plan.DurationInMonths == integerValue) ||

                (searchIsInteger && plan.MaxFreezeDays == integerValue) ||

                (searchIsInteger && plan.MaxFreezes == integerValue) ||

                (searchIsInteger && plan.GuestPassQuota == integerValue) ||

                (homeBranchOnlyMatches && plan.AccessScope == AccessScope.HomeBranchOnly) ||

                (allBranchesMatches && plan.AccessScope == AccessScope.AllBranches) ||

                (publishedMatches && plan.IsPublished) ||

                (unpublishedMatches && !plan.IsPublished));
        }

        // Get the total count for pagination.
        int totalCount = await query.CountAsync(cancellationToken);

        int page = request.Page < 1 ? 1 : request.Page;

        // Load the requested catalogue page.
        List<Plan> plans = await query
            .OrderBy(plan => plan.PlanName)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);

        // Build the Plan Catalogue items.
        List<PlanListItem> items = plans
            .Select(plan => new PlanListItem(
                plan.Id,
                plan.PlanName,
                plan.Price,
                plan.DurationInMonths,
                plan.MaxFreezeDays,
                plan.MaxFreezes,
                plan.GuestPassQuota,
                plan.AccessScope.Name,
                plan.IsPublished))
            .ToList();

        // Build the paged response.
        PagedResponse<PlanListItem> response = new()
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
        };

        return Result.Success<PagedResponse<PlanListItem>, Error>(response);
    }
}