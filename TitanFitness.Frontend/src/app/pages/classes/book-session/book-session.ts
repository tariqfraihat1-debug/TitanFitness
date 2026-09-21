import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
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
import { BookingsService } from '../../../apis/bookings/bookings.service';
import { CreateBookingDto } from '../../../apis/bookings/create-booking.dto';
import { ClassSessionDetailsDto } from '../../../apis/class-sessions/class-session-details.dto';
import { ClassSessionsService } from '../../../apis/class-sessions/class-sessions.service';
import { MemberListItemDto } from '../../../apis/members/member-list-item.dto';
import { MembersService } from '../../../apis/members/members.service';
import { AlertMessage } from '../../../shared/components/alert-message/alert-message';
import { Button } from '../../../shared/components/button/button';
import { Loading } from '../../../shared/components/loading/loading';
import { MemberSearch } from '../../../shared/components/member-search/member-search';
import { MemberSummaryCard } from '../../../shared/components/member-summary-card/member-summary-card';
import { TextareaEditor } from '../../../shared/components/textarea-editor/textarea-editor';
import { SessionSummary } from './components/session-summary/session-summary';

@Component({
  selector: 'app-book-session',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AlertMessage,
    Button,
    Loading,
    MemberSearch,
    MemberSummaryCard,
    TextareaEditor,
    SessionSummary
  ],
  templateUrl: './book-session.html'
})
export class BookSession implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly location = inject(Location);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly classSessionsService = inject(ClassSessionsService);
  private readonly membersService = inject(MembersService);
  private readonly bookingsService = inject(BookingsService);

  private readonly memberSearch$ = new Subject<string>();

  session = signal<ClassSessionDetailsDto | null>(null);
  members = signal<MemberListItemDto[]>([]);
  selectedMember = signal<MemberListItemDto | null>(null);

  private readonly sessionLoading = signal(true);
  private readonly memberLoading = signal(false);

  loading = computed(() =>
    this.sessionLoading() ||
    this.memberLoading()
  );

  searchLoading = signal(false);
  saving = signal(false);
  searched = signal(false);

  sessionError = signal('');
  memberError = signal('');
  searchError = signal('');
  bookingError = signal('');

  error = computed(() =>
    this.sessionError() ||
    this.memberError() ||
    this.searchError() ||
    this.bookingError()
  );

  canConfirm = computed(() => {
    const member = this.selectedMember();
    const session = this.session();

    return !!member &&
      !!session &&
      member.status.toLowerCase() === 'active' &&
      session.status.toLowerCase() === 'open' &&
      !this.saving();
  });

  readonly form = this.fb.group({
    trainerNotes: [
      '',
      [
        Validators.maxLength(500)
      ]
    ]
  });

  // Reads route parameters and loads the selected session and member.
  ngOnInit(): void {
    this.watchMemberSearch();

    this.route.queryParamMap
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(params => {
        const sessionId = Number(
          params.get('sessionId')
        );

        const memberId = Number(
          params.get('memberId')
        );

        if (!sessionId || sessionId < 1) {
          this.sessionLoading.set(false);

          this.sessionError.set(
            'A class session must be selected.'
          );

          return;
        }

        this.loadSession(sessionId);

        if (memberId > 0)
          this.loadPreselectedMember(memberId);
      });
  }

  // Starts a member search using the entered text.
  searchMembers(search: string): void {
    this.selectedMember.set(null);
    this.memberError.set('');
    this.bookingError.set('');

    this.memberSearch$.next(search);
  }

  // Selects a member from the search results.
  selectMember(member: MemberListItemDto): void {
    this.selectedMember.set(member);
    this.members.set([]);
    this.searched.set(false);
    this.searchError.set('');
    this.memberError.set('');
    this.bookingError.set('');
  }

  // Validates and creates the booking.
  confirmBooking(): void {
    const session = this.session();
    const member = this.selectedMember();

    if (!session || !member)
      return;

    if (member.status.toLowerCase() !== 'active') {
      this.bookingError.set(
        'Only a member with an active membership can be booked.'
      );
      return;
    }

    if (session.status.toLowerCase() !== 'open') {
      this.bookingError.set(
        'This class session is no longer open for booking.'
      );
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const booking: CreateBookingDto = {
      sessionId: session.sessionId,
      memberId: member.memberId,
      trainerNotes:
        this.form.controls.trainerNotes.value?.trim() || null
    };

    this.saving.set(true);
    this.bookingError.set('');

    this.bookingsService.createBooking(booking)
      .pipe(
        take(1),
        finalize(() =>
          this.saving.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () =>
          this.location.back(),

        error: error =>
          this.bookingError.set(error.message)
      });
  }

  // Returns to the previous screen.
  cancel(): void {
    this.location.back();
  }

  // Loads the selected class session.
  private loadSession(sessionId: number): void {
    this.sessionLoading.set(true);
    this.sessionError.set('');
    this.session.set(null);

    this.classSessionsService
      .getClassSessionById(sessionId)
      .pipe(
        take(1),
        finalize(() =>
          this.sessionLoading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response =>
          this.session.set(response.data),

        error: error =>
          this.sessionError.set(error.message)
      });
  }

  // Loads a preselected member from the route.
  private loadPreselectedMember(memberId: number): void {
    this.memberLoading.set(true);
    this.memberError.set('');

    this.membersService.getMemberById(memberId)
      .pipe(
        switchMap(detailsResponse =>
          this.membersService.getMembers(
            null,
            detailsResponse.data.membershipNumber,
            1
          )
        ),
        map(response =>
          response.data.items.find(
            member => member.memberId === memberId
          ) ?? null
        ),
        take(1),
        finalize(() =>
          this.memberLoading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: member => {
          if (!member) {
            this.memberError.set(
              'Selected member could not be loaded.'
            );
            return;
          }

          this.selectedMember.set(member);
        },

        error: error =>
          this.memberError.set(error.message)
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
