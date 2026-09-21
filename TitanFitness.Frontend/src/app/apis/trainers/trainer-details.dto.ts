import { TrainerBaseDto } from './trainer-base.dto';

export interface TrainerDetailsDto extends TrainerBaseDto {
  trainerId: number;
  branchName: string;
}
