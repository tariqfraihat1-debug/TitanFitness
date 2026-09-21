export interface MembershipDetailsDto {
  membershipId: number;
  memberId: number;
  planId: number;
  planName: string;
  purchaseDate: string;
  startDate: string;
  endDate: string;
  status: string;
  pricePaid: number;
  durationInMonths: number;
  maximumFreezeDays: number;
  maximumNumberOfFreezes: number;
  guestPassQuota: number;
  accessScope: string;
}
