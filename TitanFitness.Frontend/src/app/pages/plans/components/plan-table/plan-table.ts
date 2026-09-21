import { CurrencyPipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faTags } from '@fortawesome/free-solid-svg-icons';
import { PlanListItemDto } from '../../../../apis/plans/plan-list-item.dto';
import {
  ActionMenu,
  ActionMenuItem
} from '../../../../shared/components/action-menu/action-menu';
import { EmptyCard } from '../../../../shared/components/empty-card/empty-card';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';

export type PlanAction = 'viewPlan' | 'updatePlan';

@Component({
  selector: 'app-plan-table',
  standalone: true,
  imports: [
    CurrencyPipe,
    FontAwesomeModule,
    ActionMenu,
    EmptyCard,
    Pagination,
    StatusBadge
  ],
  templateUrl: './plan-table.html'
})
export class PlanTable {
  plans = input.required<PlanListItemDto[]>();
  currentPage = input.required<number>();
  totalPages = input.required<number>();
  totalCount = input.required<number>();
  pageSize = input(4);

  actionSelected = output<{
    action: PlanAction;
    planId: number;
  }>();

  pageChanged = output<number>();

  readonly emptyIcon = faTags;

  readonly actions: ActionMenuItem[] = [
    {
      label: 'View Plan',
      value: 'viewPlan'
    },
    {
      label: 'Update Plan',
      value: 'updatePlan'
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

  onAction(action: string, planId: number): void {
    this.actionSelected.emit({
      action: action as PlanAction,
      planId
    });
  }

  duration(months: number): string {
    return `${months} ${months === 1 ? 'month' : 'months'}`;
  }

  freezeAllowance(plan: PlanListItemDto): string {
    if (plan.maxFreezeDays === 0 && plan.maxFreezes === 0)
      return 'None';

    return `${plan.maxFreezeDays} days / ${plan.maxFreezes} freezes`;
  }

  status(plan: PlanListItemDto): string {
    return plan.isPublished
      ? 'Published'
      : 'Retired';
  }
}
