import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, forkJoin, map, switchMap } from 'rxjs';
import { MemberDetailsDto } from '../../../apis/members/member-details.dto';
import { MembersService } from '../../../apis/members/members.service';
import { ChangePlanEffectiveMode } from '../../../apis/memberships/change-plan-effective-mode';
import { MembershipDetailsDto } from '../../../apis/memberships/membership-details.dto';
import { MembershipsService } from '../../../apis/memberships/memberships.service';
import { PlanLookupItemDto } from '../../../apis/memberships/plan-lookup-item.dto';
import { PlanDetailsDto } from '../../../apis/plans/plan-details.dto';
import { PlansService } from '../../../apis/plans/plans.service';
import { AlertMessage } from '../../../shared/components/alert-message/alert-message';
import { BackNavigation } from '../../../shared/components/back-navigation/back-navigation';
import { Loading } from '../../../shared/components/loading/loading';
import { ModeBadge } from '../../../shared/components/mode-badge/mode-badge';
import { MemberSummaryStrip } from '../components/member-summary-strip/member-summary-strip';
import { CurrentMembershipSummary } from './components/current-membership-summary/current-membership-summary';
import { EffectiveDateSelector } from './components/effective-date-selector/effective-date-selector';
import { NewTermsPreview } from './components/new-terms-preview/new-terms-preview';
import { PlanSelector } from './components/plan-selector/plan-selector';

@Component({
  selector: 'app-change-membership-plan',
  standalone: true,
  imports: [
    AlertMessage,
    BackNavigation,
    Loading,
    ModeBadge,
    MemberSummaryStrip,
    CurrentMembershipSummary,
    EffectiveDateSelector,
    NewTermsPreview,
    PlanSelector
  ],
  templateUrl: './change-membership-plan.html'
})
export class ChangeMembershipPlan implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly membersService = inject(MembersService);
  private readonly membershipsService = inject(MembershipsService);
  private readonly plansService = inject(PlansService);

  membershipId = signal<number | null>(null);
  member = signal<MemberDetailsDto | null>(null);
  membership = signal<MembershipDetailsDto | null>(null);
  plans = signal<PlanLookupItemDto[]>([]);
  selectedPlanId = signal<number | null>(null);
  selectedPlan = signal<PlanDetailsDto | null>(null);
  effectiveMode = signal(ChangePlanEffectiveMode.AtRenewal);
  newStartDate = signal('');
  newEndDate = signal('');
  loading = signal(true);
  previewLoading = signal(false);
  saving = signal(false);
  error = signal('');

  // Reads the membership ID and loads the page data.
  ngOnInit(): void {
    const membershipId = Number(
      this.route.snapshot.queryParamMap.get('membershipId')
    );

    if (!Number.isInteger(membershipId) || membershipId <= 0) {
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

  // Selects a plan and loads its details for preview.
  selectPlan(planId: number | null): void {
    this.selectedPlanId.set(planId);
    this.selectedPlan.set(null);
    this.newEndDate.set('');

    if (!planId)
      return;

    this.previewLoading.set(true);

    this.plansService.getPlanById(planId)
      .pipe(
        finalize(() =>
          this.previewLoading.set(false)
        )
      )
      .subscribe({
        next: response => {
          this.selectedPlan.set(response.data);
          this.calculateDates();
        },
        error: error =>
          this.error.set(error.message)
      });
  }

  // Changes when the new plan should take effect.
  changeEffectiveMode(mode: ChangePlanEffectiveMode): void {
    this.effectiveMode.set(mode);
    this.calculateDates();
  }

  // Confirms and submits the plan change.
  confirmChange(): void {
    const membershipId = this.membershipId();
    const planId = this.selectedPlanId();

    if (!membershipId || !planId)
      return;

    this.saving.set(true);
    this.error.set('');

    this.membershipsService.changePlan(
      membershipId,
      {
        newPlanId: planId,
        effectiveMode: this.effectiveMode()
      }
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

  // Loads membership, member, and available plans.
  private loadPage(membershipId: number): void {
    this.loading.set(true);
    this.error.set('');

    this.membershipsService.getMembershipById(membershipId)
      .pipe(
        map(response => response.data),
        switchMap(membership => {
          this.membership.set(membership);
          this.calculateDates();

          return forkJoin({
            member: this.membersService
              .getMemberById(membership.memberId)
              .pipe(
                map(response => response.data)
              ),
            plans: this.membershipsService
              .getAvailablePlans(membershipId)
              .pipe(
                map(response => response.data)
              )
          });
        }),
        finalize(() =>
          this.loading.set(false)
        )
      )
      .subscribe({
        next: response => {
          this.member.set(response.member);
          this.plans.set(response.plans);
        },
        error: error =>
          this.error.set(error.message)
      });
  }

  // Calculates the new membership start and end dates.
  private calculateDates(): void {
    const membership = this.membership();

    if (!membership)
      return;

    let startDate: Date;

    if (this.effectiveMode() === ChangePlanEffectiveMode.Immediately) {
      startDate = new Date();
    } else {
      startDate = this.parseDate(membership.endDate);
      startDate.setDate(startDate.getDate() + 1);
    }

    this.newStartDate.set(
      this.toDateString(startDate)
    );

    const plan = this.selectedPlan();

    if (!plan) {
      this.newEndDate.set('');
      return;
    }

    const endDate = this.addMonths(
      startDate,
      plan.durationInMonths
    );

    endDate.setDate(
      endDate.getDate() - 1
    );

    this.newEndDate.set(
      this.toDateString(endDate)
    );
  }

  // Converts a date string into a Date object.
  private parseDate(value: string): Date {
    const [year, month, day] = value
      .split('-')
      .map(Number);

    return new Date(
      year,
      month - 1,
      day
    );
  }

  // Adds months while keeping the date valid.
  private addMonths(value: Date, months: number): Date {
    const day = value.getDate();

    const result = new Date(
      value.getFullYear(),
      value.getMonth() + months,
      1
    );

    const lastDay = new Date(
      result.getFullYear(),
      result.getMonth() + 1,
      0
    ).getDate();

    result.setDate(
      Math.min(day, lastDay)
    );

    return result;
  }

  // Formats a Date as yyyy-MM-dd.
  private toDateString(value: Date): string {
    const year = value.getFullYear();
    const month = String(
      value.getMonth() + 1
    ).padStart(2, '0');
    const day = String(
      value.getDate()
    ).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
