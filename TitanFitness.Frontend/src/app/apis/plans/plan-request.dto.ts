import { PlanMainDetailsDto } from './plan-main-details.dto';
import { PlanTermsOfferedDto } from './plan-terms-offered.dto';

export interface PlanRequestDto
  extends PlanMainDetailsDto, PlanTermsOfferedDto<number> {
}
