using TitanFitness.Domain.Common.Entities;

namespace TitanFitness.Domain.ClassSessions;

public sealed class ClassSessionStatus : Enumeration
{
    public static readonly ClassSessionStatus Open = new(1, "Open");
    public static readonly ClassSessionStatus InProgress = new(2, "In Progress");
    public static readonly ClassSessionStatus Completed = new(3, "Completed");
    public static readonly ClassSessionStatus Cancelled = new(4, "Cancelled");

    private ClassSessionStatus(int id, string name)
        : base(id, name)
    {
    }
}