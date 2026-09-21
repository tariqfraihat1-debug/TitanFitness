import { ChangePlanEffectiveMode } from './change-plan-effective-mode';

export interface ChangeMembershipPlanDto {
  newPlanId: number;
  effectiveMode: ChangePlanEffectiveMode;
}
