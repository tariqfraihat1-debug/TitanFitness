import { Component, computed, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faUserSlash } from '@fortawesome/free-solid-svg-icons';
import { TrainerListItemDto } from '../../../../apis/trainers/trainer-list-item.dto';
import {
  ActionMenu,
  ActionMenuItem
} from '../../../../shared/components/action-menu/action-menu';
import { EmptyCard } from '../../../../shared/components/empty-card/empty-card';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';

export type TrainerAction = 'viewTrainer' | 'updateTrainer';

@Component({
  selector: 'app-trainer-table',
  standalone: true,
  imports: [
    FontAwesomeModule,
    ActionMenu,
    EmptyCard,
    Pagination,
    StatusBadge
  ],
  templateUrl: './trainer-table.html'
})
export class TrainerTable {
  trainers = input.required<TrainerListItemDto[]>();
  currentPage = input.required<number>();
  totalPages = input.required<number>();
  totalCount = input.required<number>();
  pageSize = input(4);

  actionSelected = output<{
    action: TrainerAction;
    trainerId: number;
  }>();

  pageChanged = output<number>();

  readonly emptyIcon = faUserSlash;

  readonly actions: ActionMenuItem[] = [
    {
      label: 'View Trainer',
      value: 'viewTrainer'
    },
    {
      label: 'Update Trainer',
      value: 'updateTrainer'
    }
  ];

  startEntry = computed(() =>
    this.totalCount() === 0
      ? 0
      : (this.currentPage() - 1) * this.pageSize() + 1
  );

  endEntry = computed(() =>
    Math.min(
      this.currentPage() * this.pageSize(),
      this.totalCount()
    )
  );

  onAction(action: string, trainerId: number): void {
    this.actionSelected.emit({
      action: action as TrainerAction,
      trainerId
    });
  }

  trainerNumber(trainerId: number): string {
    return `#TR-${String(trainerId).padStart(4, '0')}`;
  }

  status(trainer: TrainerListItemDto): string {
    return trainer.isActive ? 'Active' : 'Inactive';
  }

  initials(name: string): string {
    return name
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map(part => part.charAt(0))
      .join('')
      .toUpperCase();
  }
}
