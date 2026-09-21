export interface MemberBaseDto {
  fullName: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  joinedDate: string;
  photo: string | null;
  homeBranchId: number;
}
