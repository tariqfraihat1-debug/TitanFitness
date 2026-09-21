import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faRightToBracket } from '@fortawesome/free-solid-svg-icons';
import {
  catchError,
  distinctUntilChanged,
  finalize,
  map,
  of,
  Subject,
  switchMap,
  take,
  tap
} from 'rxjs';
import { BranchDto } from '../../apis/branches/branch.dto';
import { BranchesService } from '../../apis/branches/branches.service';
import { CheckInsService } from '../../apis/check-ins/check-ins.service';
import { CreateCheckInDto } from '../../apis/check-ins/create-check-in.dto';
import { EntryEligibilityDto } from '../../apis/members/entry-eligibility.dto';
import { MemberListItemDto } from '../../apis/members/member-list-item.dto';
import { MembersService } from '../../apis/members/members.service';
import { AlertMessage } from '../../shared/components/alert-message/alert-message';
import { Button } from '../../shared/components/button/button';
import { Loading } from '../../shared/components/loading/loading';
import { MemberSearch } from '../../shared/components/member-search/member-search';
import { MemberSummaryCard } from '../../shared/components/member-summary-card/member-summary-card';
import { Modal } from '../../shared/components/modal/modal';
import { SelectEditor } from '../../shared/components/select-editor/select-editor';
import { CheckInDialogService } from '../../shared/services/check-in-dialog.service';
import { EntryEligibility } from './components/entry-eligibility/entry-eligibility';

@Component({
  selector: 'app-check-in',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FontAwesomeModule,
    AlertMessage,
    Button,
    Loading,
    MemberSearch,
    MemberSummaryCard,
    Modal,
    SelectEditor,
    EntryEligibility
  ],
  templateUrl: './check-in.html'
})
export class CheckIn implements OnInit {
  private readonly branchesService = inject(BranchesService);
  private readonly membersService = inject(MembersService);
  private readonly checkInsService = inject(CheckInsService);
  private readonly dialog = inject(CheckInDialogService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  private readonly memberSearch$ = new Subject<string>();
  private readonly eligibilityCheck$ =
    new Subject<{ memberId: number; branchId: number }>();

  branches = signal<BranchDto[]>([]);
  members = signal<MemberListItemDto[]>([]);
  selectedMember = signal<MemberListItemDto | null>(null);
  selectedBranchId = signal<number | null>(null);
  eligibility = signal<EntryEligibilityDto | null>(null);

  branchesLoading = signal(true);
  searchLoading = signal(false);
  eligibilityLoading = signal(false);
  saving = signal(false);
  searched = signal(false);

  branchError = signal('');
  searchError = signal('');
  eligibilityError = signal('');
  checkInError = signal('');

  error = computed(() =>
    this.branchError() ||
    this.searchError() ||
    this.eligibilityError() ||
    this.checkInError()
  );

  branchOptions = computed(() =>
    this.branches().map(branch => ({
      value: branch.branchId,
      label: branch.name
    }))
  );

  selectedBranchName = computed(() => {
    const branchId = this.selectedBranchId();

    return this.branches().find(
      branch => branch.branchId === branchId
    )?.name ?? 'the selected branch';
  });

  canConfirm = computed(() =>
    !!this.selectedMember() &&
    !!this.selectedBranchId() &&
    !!this.eligibility() &&
    !this.eligibilityLoading() &&
    !this.saving()
  );

  readonly checkInIcon = faRightToBracket;

  readonly form = this.fb.group({
    branchId: this.fb.control<number | null>(
      null,
      Validators.required
    )
  });

  // Initializes the dialog data and starts all watchers.
  ngOnInit(): void {
    this.selectedMember.set(
      this.dialog.member()
    );

    this.watchMemberSearch();
    this.watchEligibility();
    this.watchBranch();
    this.loadBranches();
  }

  // Starts a member search using the entered text.
  searchMembers(search: string): void {
    this.selectedMember.set(null);
    this.eligibility.set(null);
    this.eligibilityError.set('');
    this.checkInError.set('');

    this.memberSearch$.next(search);
  }

  // Selects a member and checks their entry eligibility.
  selectMember(member: MemberListItemDto): void {
    this.selectedMember.set(member);
    this.members.set([]);
    this.searched.set(false);
    this.searchError.set('');
    this.checkInError.set('');

    this.checkEligibility();
  }

  // Validates and creates the member check-in.
  confirmCheckIn(): void {
    const member = this.selectedMember();
    const branchId = this.selectedBranchId();

    if (!member || !branchId || !this.eligibility())
      return;

    const checkIn: CreateCheckInDto = {
      memberId: member.memberId,
      branchId
    };

    this.saving.set(true);
    this.checkInError.set('');

    this.checkInsService.createCheckIn(checkIn)
      .pipe(
        take(1),
        finalize(() =>
          this.saving.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () =>
          this.dialog.close(),

        error: error =>
          this.checkInError.set(error.message)
      });
  }

  // Closes the check-in dialog when it is not saving.
  close(): void {
    if (this.saving())
      return;

    this.dialog.close();
  }

  // Loads branches for the check-in selector.
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

  // Watches branch changes and rechecks eligibility.
  private watchBranch(): void {
    this.form.controls.branchId.valueChanges
      .pipe(
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(branchId => {
        this.selectedBranchId.set(
          branchId ? Number(branchId) : null
        );

        this.checkInError.set('');
        this.checkEligibility();
      });
  }

  // Requests an eligibility check for the selected member and branch.
  private checkEligibility(): void {
    const member = this.selectedMember();
    const branchId = this.selectedBranchId();

    this.eligibility.set(null);
    this.eligibilityError.set('');

    if (!member || !branchId) {
      this.eligibilityLoading.set(false);
      return;
    }

    this.eligibilityCheck$.next({
      memberId: member.memberId,
      branchId
    });
  }

  // Watches eligibility requests and loads the latest result.
  private watchEligibility(): void {
    this.eligibilityCheck$
      .pipe(
        tap(() => {
          this.eligibilityLoading.set(true);
          this.eligibility.set(null);
          this.eligibilityError.set('');
        }),

        switchMap(request =>
          this.membersService
            .getEntryEligibility(
              request.memberId,
              request.branchId
            )
            .pipe(
              map(response => ({
                eligibility: response.data,
                error: ''
              })),

              catchError(error =>
                of({
                  eligibility: null,
                  error: error.message
                })
              )
            )
        ),

        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(result => {
        this.eligibilityLoading.set(false);
        this.eligibility.set(result.eligibility);
        this.eligibilityError.set(result.error);
      });
  }

  // Watches member search input and loads matching members.
  private watchMemberSearch(): void {
    this.memberSearch$
      .pipe(
        map(search => search.trim()),
        distinctUntilChanged(),

        tap(search => {
          this.searchError.set('');
          this.members.set([]);

          this.searched.set(
            search.length > 0
          );

          this.searchLoading.set(
            search.length > 0
          );
        }),

        switchMap(search => {
          if (!search) {
            this.searchLoading.set(false);

            return of(
              [] as MemberListItemDto[]
            );
          }

          return this.membersService
            .getMembers(
              null,
              search,
              1
            )
            .pipe(
              map(response => {
                this.searchLoading.set(false);

                return response.data.items;
              }),

              catchError(error => {
                this.searchLoading.set(false);

                this.searchError.set(
                  error.message
                );

                return of(
                  [] as MemberListItemDto[]
                );
              })
            );
        }),

        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(members =>
        this.members.set(members)
      );
  }
}
