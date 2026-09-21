export interface PlanTermsOfferedDto<TAccessScope extends number | string = number> {
  maxFreezeDays: number;
  maxFreezes: number;
  guestPassQuota: number;
  accessScope: TAccessScope;
}
