export interface CreateBookingDto {
  sessionId: number;
  memberId: number;
  trainerNotes: string | null;
}
