import { MemberBaseDto } from './member-base.dto';

export interface MemberDetailsDto extends MemberBaseDto {
  memberId: number;
  membershipNumber: string;
}
