import { Component, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faUsersSlash } from '@fortawesome/free-solid-svg-icons';
import { MemberListItemDto } from '../../../../apis/members/member-list-item.dto';
import { ActionMenu, ActionMenuItem } from '../../../../shared/components/action-menu/action-menu';
import { EmptyCard } from '../../../../shared/components/empty-card/empty-card';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { VisitDatePipe } from '../../../../shared/pipes/visit-date-pipe';

export type MemberAction = 'viewProfile' | 'checkIn' | 'bookClass' | 'freezeMembership';

@Component({
  selector: 'app-member-table',
  standalone: true,
  imports: [FontAwesomeModule, ActionMenu, EmptyCard, StatusBadge, VisitDatePipe],
  templateUrl: './member-table.html'
})
export class MemberTable {
  members = input.required<MemberListItemDto[]>();
  actionSelected = output<{ action: MemberAction; memberId: number }>();

  readonly faUsersSlash = faUsersSlash;

  readonly actions: ActionMenuItem[] = [
    { label: 'View Profile', value: 'viewProfile' },
    { label: 'Check-In', value: 'checkIn' },
    { label: 'Book Class', value: 'bookClass' },
    { label: 'Freeze Membership', value: 'freezeMembership' }
  ];

  onAction(action: string, memberId: number): void {
    this.actionSelected.emit({
      action: action as MemberAction,
      memberId
    });
  }

  getInitials(fullName: string): string {
    const names = fullName.trim().split(/\s+/);

    if (names.length === 1)
      return names[0][0].toUpperCase();

    return `${names[0][0]}${names[names.length - 1][0]}`.toUpperCase();
  }
}
