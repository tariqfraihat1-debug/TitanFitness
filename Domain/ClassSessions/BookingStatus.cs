using TitanFitness.Domain.Common.Entities;

namespace TitanFitness.Domain.ClassSessions;

public sealed class BookingStatus : Enumeration
{
    public static readonly BookingStatus Booked = new(1, "Booked");
    public static readonly BookingStatus Waitlisted = new(2, "Waitlisted");
    public static readonly BookingStatus Attended = new(3, "Attended");
    public static readonly BookingStatus NoShow = new(4, "No Show");
    public static readonly BookingStatus Cancelled = new(5, "Cancelled");

    private BookingStatus(int id, string name)
        : base(id, name)
    {
    }
}