import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faCalendarDays,
  faEnvelope,
  faLocationDot,
  faPhone
} from '@fortawesome/free-solid-svg-icons';
import { MemberDetailsDto } from '../../../../../apis/members/member-details.dto';
import { MemberPhoto } from '../../../../../shared/components/member-photo/member-photo';
import { StatusBadge } from '../../../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-member-identity',
  standalone: true,
  imports: [
    DatePipe,
    FontAwesomeModule,
    MemberPhoto,
    StatusBadge
  ],
  templateUrl: './member-identity.html'
})
export class MemberIdentity {
  member = input.required<MemberDetailsDto>();
  status = input('No Membership');

  readonly emailIcon = faEnvelope;
  readonly phoneIcon = faPhone;
  readonly locationIcon = faLocationDot;
  readonly dateIcon = faCalendarDays;
}
