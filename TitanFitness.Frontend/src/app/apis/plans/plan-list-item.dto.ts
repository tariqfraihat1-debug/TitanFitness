import { PlanMainDetailsDto } from './plan-main-details.dto';
import { PlanTermsOfferedDto } from './plan-terms-offered.dto';

export interface PlanListItemDto
  extends PlanMainDetailsDto, PlanTermsOfferedDto<string> {
  planId: number;
}
