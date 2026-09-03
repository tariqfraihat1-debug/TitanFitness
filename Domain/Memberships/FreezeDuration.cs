using TitanFitness.Domain.Common.Entities;

namespace TitanFitness.Domain.Memberships;

public sealed class FreezeDuration : Enumeration
{
    public static readonly FreezeDuration OneMonth = new(1, "1 Month", 1);
    public static readonly FreezeDuration TwoMonths = new(2, "2 Months", 2);
    public static readonly FreezeDuration ThreeMonths = new(3, "3 Months", 3);

    public int Months { get; }

    private FreezeDuration(int id, string name, int months)
        : base(id, name)
    {
        Months = months;
    }
}