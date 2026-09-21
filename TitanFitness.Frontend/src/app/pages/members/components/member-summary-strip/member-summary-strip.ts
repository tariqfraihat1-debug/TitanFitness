import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { MemberDetailsDto } from '../../../../apis/members/member-details.dto';
import { MembershipDetailsDto } from '../../../../apis/memberships/membership-details.dto';
import { MemberPhoto } from '../../../../shared/components/member-photo/member-photo';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-member-summary-strip',
  standalone: true,
  imports: [DatePipe, MemberPhoto, StatusBadge],
  templateUrl: './member-summary-strip.html'
})
export class MemberSummaryStrip {
  member = input.required<MemberDetailsDto>();
  membership = input.required<MembershipDetailsDto>();
  variant = input<'change-plan' | 'freeze'>('change-plan');
}
