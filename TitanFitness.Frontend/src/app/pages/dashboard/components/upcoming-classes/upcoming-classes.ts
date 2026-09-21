import { Component, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCalendarXmark, faLocationDot, faUser, faUsers } from '@fortawesome/free-solid-svg-icons';
import { UpcomingClassDto } from '../../../../apis/dashboard/upcoming-class.dto';
import { Button } from '../../../../shared/components/button/button';
import { EmptyCard } from '../../../../shared/components/empty-card/empty-card';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { Time12HourPipe } from '../../../../shared/pipes/time-12-hour.pipe';

@Component({
  selector: 'app-upcoming-classes',
  standalone: true,
  imports: [
    FontAwesomeModule,
    Button,
    EmptyCard,
    StatusBadge,
    Time12HourPipe
  ],
  templateUrl: './upcoming-classes.html'
})
export class UpcomingClasses {
  classes = input.required<UpcomingClassDto[]>();
  viewSchedule = output<void>();

  readonly faCalendarXmark = faCalendarXmark;
  readonly faLocationDot = faLocationDot;
  readonly faUser = faUser;
  readonly faUsers = faUsers;
}
