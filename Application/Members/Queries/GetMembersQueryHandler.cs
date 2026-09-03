using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Common.Contract;
using TitanFitness.Application.Members.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Members.Queries;

public sealed class GetMembersQueryHandler
    : IRequestHandler<GetMembersQuery, Result<PagedResponse<MemberListItem>, Error>>
{
    private const int PageSize = 4;

    private readonly IReadOnlyRepository<Member, int> _memberReadRepository;
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IReadOnlyRepository<Membership, int> _membershipReadRepository;
    private readonly IReadOnlyRepository<CheckIn, int> _checkInReadRepository;

    public GetMembersQueryHandler(
        IReadOnlyRepository<Member, int> memberReadRepository,
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IReadOnlyRepository<Membership, int> membershipReadRepository,
        IReadOnlyRepository<CheckIn, int> checkInReadRepository)
    {
        _memberReadRepository = memberReadRepository;
        _branchReadRepository = branchReadRepository;
        _membershipReadRepository = membershipReadRepository;
        _checkInReadRepository = checkInReadRepository;
    }

    public async Task<Result<PagedResponse<MemberListItem>, Error>> Handle(
        GetMembersQuery request,
        CancellationToken cancellationToken)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        int page = request.Page < 1 ? 1 : request.Page;

        // Validate the selected branch when filtering.
        if (request.BranchId.HasValue)
        {
            bool branchExists = await _branchReadRepository.AnyAsync(
                branch => branch.Id == request.BranchId.Value,
                cancellationToken);

            if (!branchExists)
                return Result.Failure<PagedResponse<MemberListItem>, Error>(
                    Error.EntityNotFound(nameof(Branch), request.BranchId.Value));
        }

        // Load members with their branch only.
        List<MemberRow> members = await (
            from member in _memberReadRepository.GetAll()
            join branch in _branchReadRepository.GetAll()
                on member.HomeBranchId equals branch.Id
            where !request.BranchId.HasValue ||
                  member.HomeBranchId == request.BranchId.Value
            orderby member.FullName
            select new MemberRow(
                member.Id,
                member.MembershipNumber.Value,
                member.FullName,
                branch.Name))
            .ToListAsync(cancellationToken);

        if (members.Count == 0)
            return Result.Success<PagedResponse<MemberListItem>, Error>(
                CreatePagedResponse([], 0, page));

        List<int> memberIds = members
            .Select(member => member.MemberId)
            .ToList();

        // Load memberships and freezes separately.
        List<Membership> memberships = await _membershipReadRepository
            .GetAll()
            .Where(membership => memberIds.Contains(membership.MemberId))
            .Include(membership => membership.Freezes)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        // Load the latest check-in for each member.
        Dictionary<int, DateTime> lastVisits = await _checkInReadRepository
            .GetAll()
            .Where(checkIn => memberIds.Contains(checkIn.MemberId))
            .GroupBy(checkIn => checkIn.MemberId)
            .Select(group => new
            {
                MemberId = group.Key,
                LastVisit = group.Max(checkIn => checkIn.DateTime)
            })
            .ToDictionaryAsync(
                item => item.MemberId,
                item => item.LastVisit,
                cancellationToken);

        // Build the directory with live membership status.
        List<MemberListItem> items = members
            .Select(member =>
            {
                Maybe<Membership> membership = memberships
                    .Where(membership => membership.MemberId == member.MemberId)
                    .OrderByDescending(membership =>
                        membership.StartDate <= today &&
                        membership.EndDate >= today)
                    .ThenByDescending(membership => membership.StartDate)
                    .FirstOrDefault();

                string status = membership.HasNoValue
                    ? "No Membership"
                    : membership.Value.GetStatusOn(today).Name;

                DateTime? lastVisit = lastVisits.TryGetValue(
                    member.MemberId,
                    out DateTime dateTime)
                    ? dateTime
                    : null;

                return new MemberListItem(
                    member.MemberId,
                    member.MembershipNumber,
                    member.FullName,
                    status,
                    member.BranchName,
                    lastVisit);
            })
            .ToList();

        // Apply search against every value shown in the directory.
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search.Trim();
            bool searchIsDate = DateTime.TryParse(search, out DateTime searchDate);

            items = items
                .Where(member =>
                    member.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    member.MembershipNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    member.Status.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    member.Branch.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (searchIsDate &&
                     member.LastVisit.HasValue &&
                     member.LastVisit.Value.Date == searchDate.Date))
                .ToList();
        }

        int totalCount = items.Count;

        List<MemberListItem> pagedItems = items
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        PagedResponse<MemberListItem> response = CreatePagedResponse(
            pagedItems,
            totalCount,
            page);

        return Result.Success<PagedResponse<MemberListItem>, Error>(response);
    }

    private static PagedResponse<MemberListItem> CreatePagedResponse(
        List<MemberListItem> items,
        int totalCount,
        int page)
    {
        return new PagedResponse<MemberListItem>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
        };
    }

    private sealed record MemberRow(
        int MemberId,
        string MembershipNumber,
        string FullName,
        string BranchName);
}