export interface ClassSessionListItemDto {
  sessionId: number;
  className: string;
  studioName: string;
  trainerName: string;
  sessionDate: string;
  startTime: string;
  durationMinutes: number;
  capacityLimit: number;
  bookedCount: number;
  waitlistCount: number;
  status: string;
}
