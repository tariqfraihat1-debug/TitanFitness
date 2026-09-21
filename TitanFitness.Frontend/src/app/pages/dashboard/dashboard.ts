import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { faRightToBracket, faUsers } from '@fortawesome/free-solid-svg-icons';
import { catchError, finalize, forkJoin, Observable, of } from 'rxjs';
import { ApiResponse } from '../../apis/common/api-response.dto';
import { ActiveMembersDto } from '../../apis/dashboard/active-members.dto';
import { CheckInsTodayDto } from '../../apis/dashboard/check-ins-today.dto';
import { DashboardService } from '../../apis/dashboard/dashboard.service';
import { UpcomingClassDto } from '../../apis/dashboard/upcoming-class.dto';
import { AlertMessage } from '../../shared/components/alert-message/alert-message';
import { ContentHeader } from '../../shared/components/content-header/content-header';
import { Loading } from '../../shared/components/loading/loading';
import { CheckInDialogService } from '../../shared/services/check-in-dialog.service';
import { QuickAction, QuickActions } from './components/quick-actions/quick-actions';
import { StatCard } from './components/stat-card/stat-card';
import { UpcomingClasses } from './components/upcoming-classes/upcoming-classes';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    AlertMessage,
    ContentHeader,
    Loading,
    QuickActions,
    StatCard,
    UpcomingClasses
  ],
  templateUrl: './dashboard.html'
})
export class Dashboard implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly checkInDialog = inject(CheckInDialogService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  checkInsToday = signal(0);
  percentageChange = signal(0);
  activeMembers = signal(0);
  upcomingClasses = signal<UpcomingClassDto[]>([]);
  loading = signal(true);
  error = signal('');

  readonly stats = computed<{
    title: string;
    value: number;
    subtitle: string;
    icon: IconDefinition;
  }[]>(() => [
    {
      title: 'Check-ins Today',
      value: this.checkInsToday(),
      subtitle: `${this.percentageChange() >= 0 ? '+' : ''}${this.percentageChange()}% vs last week`,
      icon: faRightToBracket
    },
    {
      title: 'Active Members',
      value: this.activeMembers(),
      subtitle: `Currently on floor: ${this.activeMembers()}`,
      icon: faUsers
    }
  ]);

  // Loads the dashboard data when the page starts.
  ngOnInit(): void {
    this.loadDashboard();
  }

  // Handles navigation for dashboard quick actions.
  onQuickAction(action: QuickAction): void {
    if (action === 'newMember') {
      this.router.navigate(['/members/details'], {
        queryParams: { mode: 'create' }
      });
      return;
    }

    if (action === 'checkIn') {
      this.openCheckIn();
      return;
    }

    if (action === 'registerClass') {
      this.router.navigate(['/classes'], {
        queryParams: { booking: true }
      });
      return;
    }
  }

  // Opens the full class schedule.
  viewSchedule(): void {
    this.router.navigate(['/classes']);
  }

  // Loads all dashboard statistics and upcoming classes.
  private loadDashboard(): void {
    this.loading.set(true);
    this.error.set('');

    forkJoin({
      checkIns: this.dashboardService.getCheckInsToday()
        .pipe(
          catchError(error =>
            this.getFallback<CheckInsTodayDto>(
              {
                checkInsToday: 0,
                percentageChange: 0
              },
              error
            )
          )
        ),

      activeMembers: this.dashboardService.getActiveMembers()
        .pipe(
          catchError(error =>
            this.getFallback<ActiveMembersDto>(
              {
                activeMembers: 0
              },
              error
            )
          )
        ),

      upcomingClasses: this.dashboardService.getUpcomingClasses()
        .pipe(
          catchError(error =>
            this.getFallback<UpcomingClassDto[]>(
              [],
              error
            )
          )
        )
    })
      .pipe(
        finalize(() =>
          this.loading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        this.checkInsToday.set(
          response.checkIns.data.checkInsToday
        );

        this.percentageChange.set(
          response.checkIns.data.percentageChange
        );

        this.activeMembers.set(
          response.activeMembers.data.activeMembers
        );

        this.upcomingClasses.set(
          response.upcomingClasses.data.slice(0, 2)
        );
      });
  }

  // Returns fallback data when a dashboard request fails.
  private getFallback<T>(
    data: T,
    error: unknown
  ): Observable<ApiResponse<T>> {
    this.addError(
      error instanceof Error
        ? error.message
        : 'Something went wrong.'
    );

    return of({
      data,
      errors: []
    });
  }

  // Adds a unique error message to the dashboard.
  private addError(message: string): void {
    const current = this.error();

    if (!current) {
      this.error.set(message);
      return;
    }

    if (!current.includes(message))
      this.error.set(`${current} ${message}`);
  }

  // Opens the global check-in dialog.
  private openCheckIn(): void {
    this.checkInDialog.open();
  }
}
