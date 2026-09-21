export interface MemberListItemDto {
  memberId: number;
  membershipNumber: string;
  fullName: string;
  status: string;
  branch: string;
  lastVisit: string | null;
  photo: string | null;
}
