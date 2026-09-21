import { Component, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faLocationDot,
  faUser
} from '@fortawesome/free-solid-svg-icons';
import { ClassSessionListItemDto } from '../../../../../apis/class-sessions/class-session-list-item.dto';
import { EmptyCard } from '../../../../../shared/components/empty-card/empty-card';
import { StatusBadge } from '../../../../../shared/components/status-badge/status-badge';
import { Time12HourPipe } from '../../../../../shared/pipes/time-12-hour.pipe';

@Component({
  selector: 'app-session-list',
  standalone: true,
  imports: [
    FontAwesomeModule,
    EmptyCard,
    StatusBadge,
    Time12HourPipe
  ],
  templateUrl: './session-list.html'
})
export class SessionList {
  sessions = input.required<ClassSessionListItemDto[]>();
  totalCount = input(0);
  selectable = input(false);

  sessionSelected = output<number>();

  readonly trainerIcon = faUser;
  readonly studioIcon = faLocationDot;

  // Selects an open session when selection is enabled.
  selectSession(session: ClassSessionListItemDto): void {
    if (!this.canSelect(session))
      return;

    this.sessionSelected.emit(
      session.sessionId
    );
  }

  // Checks whether the session can be selected.
  canSelect(session: ClassSessionListItemDto): boolean {
    return this.selectable() &&
      session.status.toLowerCase() === 'open';
  }

  // Returns the exact percentage of booked capacity.
  progressPercent(session: ClassSessionListItemDto): number {
    if (session.capacityLimit <= 0)
      return 0;

    const percentage =
      session.bookedCount /
      session.capacityLimit *
      100;

    return Math.min(percentage, 100);
  }
}
