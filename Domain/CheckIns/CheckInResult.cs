using TitanFitness.Domain.Common.Entities;

namespace TitanFitness.Domain.CheckIns;

public sealed class CheckInResult : Enumeration
{
    public static readonly CheckInResult Admitted = new(1, "Admitted");
    public static readonly CheckInResult Refused = new(2, "Refused");

    private CheckInResult(int id, string name)
        : base(id, name)
    {
    }
}
