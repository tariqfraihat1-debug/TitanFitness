import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import {
  faCalendarDays,
  faChartColumn,
  faClipboardList,
  faDumbbell,
  faRightToBracket,
  faUsers,
  faXmark
} from '@fortawesome/free-solid-svg-icons';
import { CheckInDialogService } from '../../services/check-in-dialog.service';
import { Button } from '../button/button';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    FontAwesomeModule,
    Button
  ],
  templateUrl: './sidebar.html'
})
export class Sidebar {
  private readonly checkInDialog = inject(CheckInDialogService);

  readonly faRightToBracket = faRightToBracket;
  readonly faXmark = faXmark;

  readonly items: {
    label: string;
    link: string;
    icon: IconDefinition;
  }[] = [
      {
        label: 'Dashboard',
        link: '/dashboard',
        icon: faChartColumn
      },
      {
        label: 'Members',
        link: '/members',
        icon: faUsers
      },
      {
        label: 'Classes',
        link: '/classes',
        icon: faCalendarDays
      },
      {
        label: 'Trainers',
        link: '/trainers',
        icon: faDumbbell
      },
      {
        label: 'Plans',
        link: '/plans',
        icon: faClipboardList
      }
    ];

  openCheckIn(): void {
    this.checkInDialog.open();
  }
}
