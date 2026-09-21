import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { catchError, finalize, of } from 'rxjs';
import { AccessScopeFilter } from '../../apis/plans/access-scope-filter';
import { PlanListItemDto } from '../../apis/plans/plan-list-item.dto';
import { PlansService } from '../../apis/plans/plans.service';
import { AlertMessage } from '../../shared/components/alert-message/alert-message';
import { Button } from '../../shared/components/button/button';
import { ContentHeader } from '../../shared/components/content-header/content-header';
import { Loading } from '../../shared/components/loading/loading';
import { PlanAction, PlanTable } from './components/plan-table/plan-table';

@Component({
  selector: 'app-plans',
  standalone: true,
  imports: [
    FontAwesomeModule,
    AlertMessage,
    Button,
    ContentHeader,
    Loading,
    PlanTable
  ],
  templateUrl: './plans.html'
})
export class Plans implements OnInit {
  private readonly plansService = inject(PlansService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  plans = signal<PlanListItemDto[]>([]);

  accessScope = signal<AccessScopeFilter | null>(null);
  search = signal('');
  page = signal(1);
  pageSize = signal(4);
  totalPages = signal(0);
  totalCount = signal(0);

  loading = signal(true);
  error = signal('');

  readonly addIcon = faPlus;
  readonly AccessScopeFilter = AccessScopeFilter;

  // Reads filters from the URL and loads the plans.
  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(params => {
        const scope = Number(
          params.get('accessScope')
        );

        this.accessScope.set(
          scope === AccessScopeFilter.HomeBranchOnly ||
            scope === AccessScopeFilter.AllBranches
            ? scope
            : null
        );

        this.search.set(
          params.get('search') ?? ''
        );

        this.page.set(
          Number(params.get('page')) || 1
        );

        this.loadPlans();
      });
  }

  // Updates the access scope filter in the URL.
  onAccessScopeChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;

    this.router.navigate(['/plans'], {
      queryParams: {
        accessScope: value || null,
        page: 1
      },
      queryParamsHandling: 'merge'
    });
  }

  // Changes the current catalogue page.
  onPageChange(page: number): void {
    this.router.navigate(['/plans'], {
      queryParams: { page },
      queryParamsHandling: 'merge'
    });
  }

  // Opens the plan details page in create mode.
  addPlan(): void {
    this.router.navigate(['/plans/details'], {
      queryParams: {
        mode: 'create'
      }
    });
  }

  // Opens the selected plan in view or edit mode.
  onPlanAction(event: {
    action: PlanAction;
    planId: number;
  }): void {
    if (event.action === 'viewPlan') {
      this.router.navigate(['/plans/details'], {
        queryParams: {
          mode: 'view',
          planId: event.planId
        }
      });
      return;
    }

    this.router.navigate(['/plans/details'], {
      queryParams: {
        mode: 'edit',
        planId: event.planId
      }
    });
  }

  // Loads the filtered and paged plan catalogue.
  private loadPlans(): void {
    this.loading.set(true);
    this.error.set('');
    this.plans.set([]);
    this.totalPages.set(0);
    this.totalCount.set(0);

    this.plansService.getPlans(
      this.accessScope(),
      this.search(),
      this.page()
    )
      .pipe(
        catchError(error => {
          this.error.set(error.message);

          return of({
            data: {
              items: [],
              totalCount: 0,
              pageNumber: this.page(),
              pageSize: 4,
              totalPages: 0
            },
            errors: []
          });
        }),
        finalize(() =>
          this.loading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        this.plans.set(
          response.data.items
        );

        this.pageSize.set(
          response.data.pageSize
        );

        this.totalCount.set(
          response.data.totalCount
        );

        this.totalPages.set(
          response.data.totalPages
        );
      });
  }
}
