export interface CurrentMembershipDto {
  membershipId: number;
  planId: number;
  planName: string;
  pricePaid: number;
  startDate: string;
  endDate: string;
  status: string;
  freezesUsed: number;
  freezesAllowed: number;
  guestPassesUsed: number;
  guestPassesAllowed: number;
}
