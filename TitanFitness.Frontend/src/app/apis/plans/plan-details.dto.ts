import { PlanListItemDto } from './plan-list-item.dto';

export interface PlanDetailsDto extends PlanListItemDto {
  activeMembershipsCount: number;
}
