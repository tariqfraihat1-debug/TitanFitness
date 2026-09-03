using TitanFitness.Domain.Common.Entities;

namespace TitanFitness.Domain.Memberships;

public sealed class FreezeReason : Enumeration
{
    public static readonly FreezeReason ExtendedTravel = new(1, "Extended Travel");
    public static readonly FreezeReason Injury = new(2, "Injury");
    public static readonly FreezeReason Other = new(3, "Other");

    private FreezeReason(int id, string name)
        : base(id, name)
    {
    }
}