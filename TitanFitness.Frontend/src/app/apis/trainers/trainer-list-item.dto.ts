import { TrainerLookupItemDto } from './trainer-lookup-item.dto';

export interface TrainerListItemDto extends TrainerLookupItemDto {
  branchName: string;
  isActive: boolean;
}
