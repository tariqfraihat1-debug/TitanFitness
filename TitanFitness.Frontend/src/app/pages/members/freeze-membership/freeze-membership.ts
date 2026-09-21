import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, map, switchMap } from 'rxjs';
import { MemberDetailsDto } from '../../../apis/members/member-details.dto';
import { MembersService } from '../../../apis/members/members.service';
import { AddFreezeDto } from '../../../apis/memberships/add-freeze.dto';
import { MembershipDetailsDto } from '../../../apis/memberships/membership-details.dto';
import { MembershipsService } from '../../../apis/memberships/memberships.service';
import { AlertMessage } from '../../../shared/components/alert-message/alert-message';
import { BackNavigation } from '../../../shared/components/back-navigation/back-navigation';
import { Loading } from '../../../shared/components/loading/loading';
import { MemberSummaryStrip } from '../components/member-summary-strip/member-summary-strip';
import { FreezeParameters } from './components/freeze-parameters/freeze-parameters';
import { ProjectedImpact } from './components/projected-impact/projected-impact';

@Component({
  selector: 'app-freeze-membership',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AlertMessage,
    BackNavigation,
    Loading,
    MemberSummaryStrip,
    FreezeParameters,
    ProjectedImpact
  ],
  templateUrl: './freeze-membership.html'
})
export class FreezeMembership implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly membersService = inject(MembersService);
  private readonly membershipsService = inject(MembershipsService);

  membershipId = signal<number | null>(null);
  member = signal<MemberDetailsDto | null>(null);
  membership = signal<MembershipDetailsDto | null>(null);
  loading = signal(true);
  saving = signal(false);
  error = signal('');

  readonly durationOptions = [
    { id: 1, label: '1 Month', months: 1 },
    { id: 2, label: '2 Months', months: 2 },
    { id: 3, label: '3 Months', months: 3 }
  ];

  readonly reasonOptions = [
    { id: 1, label: 'Extended Travel' },
    { id: 2, label: 'Injury' },
    { id: 3, label: 'Other' }
  ];

  readonly minStartDate = this.today();

  readonly form = this.fb.group({
    startDate: [this.today(), Validators.required],
    freezeDurationId: [
      null as number | null,
      Validators.required
    ],
    freezeReasonId: [
      null as number | null,
      Validators.required
    ],
    notes: ['', [
      Validators.maxLength(200)
    ]]
  });

  private readonly formValue = toSignal(
    this.form.valueChanges,
    {
      initialValue: this.form.getRawValue()
    }
  );

  readonly selectedDuration = computed(() => {
    const durationId = this.formValue().freezeDurationId;

    return this.durationOptions.find(
      duration => duration.id === durationId
    ) ?? null;
  });

  readonly projectedEndDate = computed(() => {
    const membership = this.membership();
    const startDate = this.formValue().startDate;
    const duration = this.selectedDuration();

    if (!membership || !startDate || !duration)
      return '';

    const freezeStart = this.parseDate(startDate);
    const freezeEnd = this.addMonths(
      freezeStart,
      duration.months
    );

    freezeEnd.setUTCDate(
      freezeEnd.getUTCDate() - 1
    );

    const frozenDays =
      Math.round(
        (freezeEnd.getTime() - freezeStart.getTime()) /
        86400000
      ) + 1;

    const currentEnd = this.parseDate(
      membership.endDate
    );

    currentEnd.setUTCDate(
      currentEnd.getUTCDate() + frozenDays
    );

    return this.toDateString(currentEnd);
  });

  // Reads the membership ID and loads the freeze page data.
  ngOnInit(): void {
    const membershipId = Number(
      this.route.snapshot.queryParamMap.get(
        'membershipId'
      )
    );

    if (
      !Number.isInteger(membershipId) ||
      membershipId <= 0
    ) {
      this.router.navigate(['/members']);
      return;
    }

    this.membershipId.set(membershipId);
    this.loadPage(membershipId);
  }

  // Returns to the member profile or members list.
  back(): void {
    const memberId = this.member()?.memberId;

    if (memberId) {
      this.router.navigate(['/members/profile'], {
        queryParams: { memberId }
      });
      return;
    }

    this.router.navigate(['/members']);
  }

  // Validates and submits the membership freeze.
  confirmFreeze(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const membershipId = this.membershipId();

    if (!membershipId)
      return;

    const value = this.form.getRawValue();

    const freeze: AddFreezeDto = {
      startDate: value.startDate!,
      freezeDurationId: value.freezeDurationId!,
      freezeReasonId: value.freezeReasonId!,
      notes: value.notes?.trim() || null
    };

    this.saving.set(true);
    this.error.set('');

    this.membershipsService.addFreeze(
      membershipId,
      freeze
    )
      .pipe(
        finalize(() =>
          this.saving.set(false)
        )
      )
      .subscribe({
        next: () => this.back(),
        error: error =>
          this.error.set(error.message)
      });
  }

  // Loads the membership and its member details.
  private loadPage(membershipId: number): void {
    this.loading.set(true);
    this.error.set('');

    this.membershipsService
      .getMembershipById(membershipId)
      .pipe(
        map(response => response.data),
        switchMap(membership => {
          this.membership.set(membership);

          return this.membersService.getMemberById(
            membership.memberId
          );
        }),
        finalize(() =>
          this.loading.set(false)
        )
      )
      .subscribe({
        next: response =>
          this.member.set(response.data),
        error: error =>
          this.error.set(error.message)
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

  // Converts a date string into a UTC Date.
  private parseDate(value: string): Date {
    const [year, month, day] = value
      .split('-')
      .map(Number);

    return new Date(
      Date.UTC(
        year,
        month - 1,
        day
      )
    );
  }

  // Adds months while keeping the date valid.
  private addMonths(
    value: Date,
    months: number
  ): Date {
    const day = value.getUTCDate();

    const result = new Date(
      Date.UTC(
        value.getUTCFullYear(),
        value.getUTCMonth() + months,
        1
      )
    );

    const lastDay = new Date(
      Date.UTC(
        result.getUTCFullYear(),
        result.getUTCMonth() + 1,
        0
      )
    ).getUTCDate();

    result.setUTCDate(
      Math.min(day, lastDay)
    );

    return result;
  }

  // Formats a UTC Date as yyyy-MM-dd.
  private toDateString(value: Date): string {
    const year = value.getUTCFullYear();
    const month = String(
      value.getUTCMonth() + 1
    ).padStart(2, '0');
    const day = String(
      value.getUTCDate()
    ).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
