namespace TitanFitness.Application.Bookings.Contract;

public sealed record CreateBookingRequest(
    int SessionId,
    int MemberId,
    string? TrainerNotes);