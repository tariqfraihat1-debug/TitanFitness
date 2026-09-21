import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { BranchDto } from '../../apis/branches/branch.dto';
import { BranchesService } from '../../apis/branches/branches.service';
import { MemberListItemDto } from '../../apis/members/member-list-item.dto';
import { MembersService } from '../../apis/members/members.service';
import { AlertMessage } from '../../shared/components/alert-message/alert-message';
import { BranchSelect } from '../../shared/components/branch-select/branch-select';
import { Button } from '../../shared/components/button/button';
import { ContentHeader } from '../../shared/components/content-header/content-header';
import { Loading } from '../../shared/components/loading/loading';
import { Pagination } from '../../shared/components/pagination/pagination';
import { CheckInDialogService } from '../../shared/services/check-in-dialog.service';
import { MemberAction, MemberTable } from './components/member-table/member-table';

@Component({
  selector: 'app-members',
  standalone: true,
  imports: [
    AlertMessage,
    BranchSelect,
    Button,
    ContentHeader,
    Loading,
    Pagination,
    MemberTable
  ],
  templateUrl: './members.html'
})
export class Members implements OnInit {
  private readonly membersService = inject(MembersService);
  private readonly branchesService = inject(BranchesService);
  private readonly checkInDialog = inject(CheckInDialogService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  members = signal<MemberListItemDto[]>([]);
  branches = signal<BranchDto[]>([]);
  branchId = signal<number | null>(null);
  search = signal('');
  page = signal(1);
  pageSize = signal(4);
  totalPages = signal(0);
  totalCount = signal(0);

  private readonly branchesLoading = signal(true);
  private readonly membersLoading = signal(true);

  loading = computed(() =>
    this.branchesLoading() ||
    this.membersLoading()
  );

  branchError = signal('');
  memberError = signal('');

  error = computed(() =>
    this.branchError() ||
    this.memberError()
  );

  readonly startEntry = computed(() =>
    this.totalCount() === 0
      ? 0
      : (this.page() - 1) * this.pageSize() + 1
  );

  readonly endEntry = computed(() =>
    Math.min(
      this.page() * this.pageSize(),
      this.totalCount()
    )
  );

  // Loads branches and reads member filters from the URL.
  ngOnInit(): void {
    this.loadBranches();

    this.route.queryParamMap
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(params => {
        const branchId = params.get('branchId');

        this.branchId.set(
          branchId ? Number(branchId) : null
        );

        this.search.set(
          params.get('search') ?? ''
        );

        this.page.set(
          Number(params.get('page')) || 1
        );

        this.loadMembers();
      });
  }

  // Updates the selected branch filter.
  onBranchChange(branchId: number | null): void {
    this.router.navigate(['/members'], {
      queryParams: {
        branchId,
        page: 1
      },
      queryParamsHandling: 'merge'
    });
  }

  // Changes the current member page.
  onPageChange(page: number): void {
    this.router.navigate(['/members'], {
      queryParams: { page },
      queryParamsHandling: 'merge'
    });
  }

  // Opens the member details page in create mode.
  addMember(): void {
    this.router.navigate(['/members/details'], {
      queryParams: { mode: 'create' }
    });
  }

  // Handles the selected action for a member.
  onMemberAction(event: { action: MemberAction; memberId: number }): void {
    if (event.action === 'viewProfile') {
      this.router.navigate(['/members/profile'], {
        queryParams: { memberId: event.memberId }
      });
      return;
    }

    if (event.action === 'checkIn') {
      this.openCheckIn(event.memberId);
      return;
    }

    if (event.action === 'bookClass') {
      this.router.navigate(['/classes'], {
        queryParams: {
          booking: true,
          memberId: event.memberId
        }
      });
      return;
    }

    this.openFreeze(event.memberId);
  }

  // Loads the current membership before opening freeze.
  private openFreeze(memberId: number): void {
    this.memberError.set('');

    this.membersService.getCurrentMembership(memberId)
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          this.router.navigate(['/members/freeze'], {
            queryParams: {
              memberId,
              membershipId: response.data.membershipId
            }
          });
        },
        error: error =>
          this.memberError.set(error.message)
      });
  }

  // Loads branches for the member filter.
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

  // Loads the filtered and paged member list.
  private loadMembers(): void {
    this.membersLoading.set(true);
    this.memberError.set('');
    this.members.set([]);
    this.totalPages.set(0);
    this.totalCount.set(0);

    this.membersService.getMembers(
      this.branchId(),
      this.search(),
      this.page()
    )
      .pipe(
        catchError(error => {
          this.memberError.set(error.message);

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
          this.membersLoading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        this.members.set(response.data.items);
        this.pageSize.set(response.data.pageSize);
        this.totalCount.set(response.data.totalCount);
        this.totalPages.set(response.data.totalPages);
      });
  }

  // Opens the check-in dialog for the selected member.
  private openCheckIn(memberId: number): void {
    const member = this.members().find(
      member => member.memberId === memberId
    );

    if (!member)
      return;

    this.checkInDialog.open(member);
  }
}
