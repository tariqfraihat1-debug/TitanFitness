import { MemberBaseDto } from './member-base.dto';

export interface CreateMemberDto extends MemberBaseDto {
  membershipNumber: string | null;
}
