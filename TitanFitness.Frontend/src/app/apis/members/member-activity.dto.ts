export interface MemberActivityDto {
  activityType: string;
  description: string;
  dateTime: string;
  result: string | null;
  refusalReason: string | null;
}
