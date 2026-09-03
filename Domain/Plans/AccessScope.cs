using TitanFitness.Domain.Common.Entities;

namespace TitanFitness.Domain.Plans;

public sealed class AccessScope : Enumeration
{
    public static readonly AccessScope HomeBranchOnly = new(1, "Home Branch Only");
    public static readonly AccessScope AllBranches = new(2, "All Branches");

    private AccessScope(int id, string name)
        : base(id, name)
    {
    }
}