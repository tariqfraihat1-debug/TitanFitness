import { DecimalPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faCalendarCheck,
  faUsers
} from '@fortawesome/free-solid-svg-icons';
import { ClassSessionDaySummaryDto } from '../../../../../apis/class-sessions/class-session-day-summary.dto';

@Component({
  selector: 'app-capacity-overview',
  standalone: true,
  imports: [DecimalPipe, FontAwesomeModule],
  templateUrl: './capacity-overview.html'
})
export class CapacityOverview {
  summary = input.required<ClassSessionDaySummaryDto>();

  readonly bookingsIcon = faUsers;
  readonly fillRateIcon = faCalendarCheck;
}
