export interface AddFreezeDto {
  startDate: string;
  freezeDurationId: number;
  freezeReasonId: number;
  notes: string | null;
}
