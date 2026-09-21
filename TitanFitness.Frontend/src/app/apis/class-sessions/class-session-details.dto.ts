import { ClassSessionListItemDto } from './class-session-list-item.dto';

export interface ClassSessionDetailsDto extends ClassSessionListItemDto {
  branchId: number;
  studioId: number;
  trainerId: number;
  remainingPlaces: number;
  description: string | null;
}
