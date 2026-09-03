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

        // Build the directory with branch, relevant membership and last visit.
        var query =
            from member in _memberReadRepository.GetAll()

            join branch in _branchReadRepository.GetAll()
                on member.HomeBranchId equals branch.Id

            let membership = (
                from membership in _membershipReadRepository.GetAll()
                where membership.MemberId == member.Id
                orderby
                    membership.StartDate <= today &&
                    membership.EndDate >= today descending,
                    membership.StartDate descending
                select membership)
                .FirstOrDefault()

            let lastVisit = (
                from checkIn in _checkInReadRepository.GetAll()
                where checkIn.MemberId == member.Id
                orderby checkIn.DateTime descending
                select (DateTime?)checkIn.DateTime)
                .FirstOrDefault()

            where !request.BranchId.HasValue ||
                  member.HomeBranchId == request.BranchId.Value

            select new
            {
                member.Id,
                MembershipNumber = member.MembershipNumber.Value,
                member.FullName,
                BranchName = branch.Name,
                Membership = membership,
                IsFrozen = membership != null &&
                    membership.Freezes.Any(freeze =>
                        freeze.StartDate <= today &&
                        freeze.EndDate >= today),
                LastVisit = lastVisit
            };

        // Apply search using the live membership status.
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search.Trim();
            bool searchIsDate = DateTime.TryParse(search, out DateTime searchDate);
            DateTime searchDateStart = searchDate.Date;
            DateTime searchDateEnd = searchDateStart.AddDays(1);

            query = query.Where(member =>
                member.FullName.Contains(search) ||
                member.MembershipNumber.Contains(search) ||
                member.BranchName.Contains(search) ||

                (member.Membership == null &&
                 "No Membership".Contains(search)) ||

                (member.Membership != null &&
                 member.Membership.Status == MembershipStatus.Cancelled &&
                 "Cancelled".Contains(search)) ||

                (member.Membership != null &&
                 member.Membership.Status != MembershipStatus.Cancelled &&
                 member.Membership.StartDate > today &&
                 "Pending".Contains(search)) ||

                (member.Membership != null &&
                 member.Membership.Status != MembershipStatus.Cancelled &&
                 member.Membership.EndDate < today &&
                 "Expired".Contains(search)) ||

                (member.Membership != null &&
                 member.Membership.Status != MembershipStatus.Cancelled &&
                 member.Membership.StartDate <= today &&
                 member.Membership.EndDate >= today &&
                 member.IsFrozen &&
                 "Frozen".Contains(search)) ||

                (member.Membership != null &&
                 member.Membership.Status != MembershipStatus.Cancelled &&
                 member.Membership.StartDate <= today &&
                 member.Membership.EndDate >= today &&
                 !member.IsFrozen &&
                 "Active".Contains(search)) ||

                (searchIsDate &&
                 member.LastVisit.HasValue &&
                 member.LastVisit.Value >= searchDateStart &&
                 member.LastVisit.Value < searchDateEnd));
        }

        // Get the total count for pagination.
        int totalCount = await query.CountAsync(cancellationToken);

        int page = request.Page < 1 ? 1 : request.Page;

        // Load the requested page.
        var members = await query
            .OrderBy(member => member.FullName)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);

        // Build the live membership status.
        List<MemberListItem> items = members
            .Select(member =>
            {
                string status;

                if (member.Membership == null)
                    status = "No Membership";
                else if (member.Membership.Status == MembershipStatus.Cancelled)
                    status = MembershipStatus.Cancelled.Name;
                else if (member.Membership.StartDate > today)
                    status = MembershipStatus.Pending.Name;
                else if (member.Membership.EndDate < today)
                    status = MembershipStatus.Expired.Name;
                else if (member.IsFrozen)
                    status = MembershipStatus.Frozen.Name;
                else
                    status = MembershipStatus.Active.Name;

                return new MemberListItem(
                    member.Id,
                    member.MembershipNumber,
                    member.FullName,
                    status,
                    member.BranchName,
                    member.LastVisit);
            })
            .ToList();

        // Build the paged response.
        PagedResponse<MemberListItem> response = new()
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
        };

        return Result.Success<PagedResponse<MemberListItem>, Error>(response);
    }
}