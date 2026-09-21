import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { BranchDto } from '../../../apis/branches/branch.dto';
import { BranchesService } from '../../../apis/branches/branches.service';
import { ClassSessionDaySummaryDto } from '../../../apis/class-sessions/class-session-day-summary.dto';
import { ClassSessionListItemDto } from '../../../apis/class-sessions/class-session-list-item.dto';
import { ClassSessionsService } from '../../../apis/class-sessions/class-sessions.service';
import { AlertMessage } from '../../../shared/components/alert-message/alert-message';
import { ContentHeader } from '../../../shared/components/content-header/content-header';
import { Loading } from '../../../shared/components/loading/loading';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { AddClass } from '../add-class/add-class';
import { CapacityOverview } from './components/capacity-overview/capacity-overview';
import { ScheduleFilters } from './components/schedule-filters/schedule-filters';
import { SessionList } from './components/session-list/session-list';

@Component({
  selector: 'app-class-schedule',
  standalone: true,
  imports: [
    AlertMessage,
    ContentHeader,
    Loading,
    Pagination,
    AddClass,
    CapacityOverview,
    ScheduleFilters,
    SessionList
  ],
  templateUrl: './class-schedule.html'
})
export class ClassSchedule implements OnInit {
  private readonly branchesService = inject(BranchesService);
  private readonly classSessionsService = inject(ClassSessionsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  branches = signal<BranchDto[]>([]);
  sessions = signal<ClassSessionListItemDto[]>([]);

  summary = signal<ClassSessionDaySummaryDto>({
    totalBookings: 0,
    averageFillRate: 0
  });

  branchId = signal<number | null>(null);
  date = signal('');
  page = signal(1);
  totalPages = signal(0);
  totalCount = signal(0);

  bookingMode = signal(false);
  bookingMemberId = signal<number | null>(null);

  showAddClass = signal(false);

  private readonly branchesLoading = signal(true);
  private readonly scheduleLoading = signal(true);

  loading = computed(() =>
    this.branchesLoading() ||
    this.scheduleLoading()
  );

  branchError = signal('');
  scheduleError = signal('');
  summaryError = signal('');

  error = computed(() =>
    this.branchError() ||
    this.scheduleError() ||
    this.summaryError()
  );

  // Loads route state and fetches the initial schedule.
  ngOnInit(): void {
    this.loadBranches();

    this.route.queryParamMap
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(params => {
        const branchId = params.get('branchId');
        const date = params.get('date');
        const memberId = Number(params.get('memberId'));

        this.branchId.set(
          branchId ? Number(branchId) : null
        );

        this.date.set(
          date || this.today()
        );

        this.page.set(
          Number(params.get('page')) || 1
        );

        this.bookingMode.set(
          params.get('booking') === 'true'
        );

        this.bookingMemberId.set(
          memberId > 0 ? memberId : null
        );

        this.loadSchedule();
      });
  }

  // Updates the selected branch in the URL.
  onBranchChange(branchId: number | null): void {
    this.router.navigate(['/classes'], {
      queryParams: {
        branchId,
        page: 1
      },
      queryParamsHandling: 'merge'
    });
  }

  // Updates the selected schedule date.
  onDateChange(date: string): void {
    this.router.navigate(['/classes'], {
      queryParams: {
        date,
        page: 1
      },
      queryParamsHandling: 'merge'
    });
  }

  // Changes the current schedule page.
  onPageChange(page: number): void {
    this.router.navigate(['/classes'], {
      queryParams: { page },
      queryParamsHandling: 'merge'
    });
  }

  // Opens the booking page for the selected session.
  selectBookingSession(sessionId: number): void {
    if (!this.bookingMode())
      return;

    const queryParams: {
      sessionId: number;
      memberId?: number;
    } = {
      sessionId
    };

    const memberId = this.bookingMemberId();

    if (memberId)
      queryParams.memberId = memberId;

    this.router.navigate(
      ['/classes/book-session'],
      { queryParams }
    );
  }

  // Opens the Add Class modal.
  addClass(): void {
    this.showAddClass.set(true);
  }

  // Closes the Add Class modal.
  closeAddClass(): void {
    this.showAddClass.set(false);
  }

  // Refreshes or navigates to the newly created class date.
  onClassCreated(event: { branchId: number; date: string }): void {
    this.showAddClass.set(false);

    if (
      this.branchId() === event.branchId &&
      this.date() === event.date &&
      this.page() === 1
    ) {
      this.loadSchedule();
      return;
    }

    this.router.navigate(['/classes'], {
      queryParams: {
        branchId: event.branchId,
        date: event.date,
        page: 1
      },
      queryParamsHandling: 'merge'
    });
  }

  // Loads the branches used by the schedule filter.
  private loadBranches(): void {
    this.branchesLoading.set(true);
    this.branchError.set('');

    this.branchesService.getBranches()
      .pipe(
        catchError(error => {
          this.branchError.set(error.message);

          return of({
            data: [],
            errors: []
          });
        }),
        finalize(() =>
          this.branchesLoading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response =>
        this.branches.set(response.data)
      );
  }

  // Loads sessions and the daily capacity summary together.
  private loadSchedule(): void {
    this.scheduleLoading.set(true);
    this.scheduleError.set('');
    this.summaryError.set('');

    forkJoin({
      sessions: this.classSessionsService
        .getClassSessions(
          this.branchId(),
          this.date(),
          this.page()
        )
        .pipe(
          catchError(error => {
            this.scheduleError.set(error.message);

            return of({
              data: {
                items: [],
                totalCount: 0,
                pageNumber: this.page(),
                pageSize: 3,
                totalPages: 0
              },
              errors: []
            });
          })
        ),

      summary: this.classSessionsService
        .getDaySummary(
          this.branchId(),
          this.date()
        )
        .pipe(
          catchError(error => {
            this.summaryError.set(error.message);

            return of({
              data: {
                totalBookings: 0,
                averageFillRate: 0
              },
              errors: []
            });
          })
        )
    })
      .pipe(
        finalize(() =>
          this.scheduleLoading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        this.sessions.set(
          response.sessions.data.items
        );

        this.totalCount.set(
          response.sessions.data.totalCount
        );

        this.totalPages.set(
          response.sessions.data.totalPages
        );

        this.summary.set(
          response.summary.data
        );
      });
  }

  // Returns today's date in yyyy-MM-dd format.
  private today(): string {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(
      today.getMonth() + 1
    ).padStart(2, '0');
    const day = String(
      today.getDate()
    ).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
