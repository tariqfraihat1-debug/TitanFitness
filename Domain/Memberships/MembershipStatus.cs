using TitanFitness.Domain.Common.Entities;

namespace TitanFitness.Domain.Memberships;

public sealed class MembershipStatus : Enumeration
{
    public static readonly MembershipStatus Pending = new(1, "Pending");
    public static readonly MembershipStatus Active = new(2, "Active");
    public static readonly MembershipStatus Frozen = new(3, "Frozen");
    public static readonly MembershipStatus Expired = new(4, "Expired");
    public static readonly MembershipStatus Cancelled = new(5, "Cancelled");

    private MembershipStatus(int id, string name)
        : base(id, name)
    {
    }
}