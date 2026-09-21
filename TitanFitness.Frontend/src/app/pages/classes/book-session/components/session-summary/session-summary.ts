import { Component, input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faClock,
  faLocationDot,
  faUser,
  faUsers
} from '@fortawesome/free-solid-svg-icons';
import { ClassSessionDetailsDto } from '../../../../../apis/class-sessions/class-session-details.dto';

@Component({
  selector: 'app-session-summary',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './session-summary.html'
})
export class SessionSummary {
  session = input.required<ClassSessionDetailsDto>();

  readonly clockIcon = faClock;
  readonly trainerIcon = faUser;
  readonly studioIcon = faLocationDot;
  readonly capacityIcon = faUsers;

  startTime(): string {
    return this.session().startTime.slice(0, 5);
  }

  endTime(): string {
    const [hours, minutes] = this.session()
      .startTime
      .split(':')
      .map(Number);

    const date = new Date();
    date.setHours(
      hours,
      minutes + this.session().durationMinutes,
      0,
      0
    );

    return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
  }

  dayLabel(): string {
    const sessionDate = new Date(
      `${this.session().sessionDate}T00:00:00`
    );

    const today = new Date();

    if (
      sessionDate.getFullYear() === today.getFullYear() &&
      sessionDate.getMonth() === today.getMonth() &&
      sessionDate.getDate() === today.getDate()
    )
      return 'Today';

    return sessionDate.toLocaleDateString();
  }
}
