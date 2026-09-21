import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCheck, faPen } from '@fortawesome/free-solid-svg-icons';
import { finalize, take } from 'rxjs';
import { AccessScopeFilter } from '../../../apis/plans/access-scope-filter';
import { PlanDetailsDto } from '../../../apis/plans/plan-details.dto';
import { PlanRequestDto } from '../../../apis/plans/plan-request.dto';
import { PlansService } from '../../../apis/plans/plans.service';
import { AlertMessage } from '../../../shared/components/alert-message/alert-message';
import { BackNavigation } from '../../../shared/components/back-navigation/back-navigation';
import { Button } from '../../../shared/components/button/button';
import { Loading } from '../../../shared/components/loading/loading';
import { ModeBadge } from '../../../shared/components/mode-badge/mode-badge';
import { PlanMainDetails } from './components/plan-main-details/plan-main-details';
import { PlanTermsOffered } from './components/plan-terms-offered/plan-terms-offered';

type PlanMode = 'create' | 'view' | 'edit';

@Component({
  selector: 'app-plan-details',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FontAwesomeModule,
    AlertMessage,
    BackNavigation,
    Button,
    Loading,
    ModeBadge,
    PlanMainDetails,
    PlanTermsOffered
  ],
  templateUrl: './plan-details.html'
})
export class PlanDetails implements OnInit {
  private readonly plansService = inject(PlansService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  plan = signal<PlanDetailsDto | null>(null);

  mode = signal<PlanMode>('create');
  planId = signal<number | null>(null);

  loading = signal(false);
  saving = signal(false);

  loadError = signal('');
  saveError = signal('');

  readonly editIcon = faPen;
  readonly saveIcon = faCheck;

  readonly form = this.fb.group({
    planName: this.fb.control<string | null>('', [
      Validators.required,
      Validators.maxLength(50)
    ]),
    price: this.fb.control<number | null>(0, [
      Validators.required,
      Validators.min(0)
    ]),
    durationInMonths: this.fb.control<number | null>(null, [
      Validators.required,
      Validators.min(1)
    ]),
    maxFreezeDays: this.fb.control<number | null>(
      0,
      Validators.min(0)
    ),
    maxFreezes: this.fb.control<number | null>(
      0,
      Validators.min(0)
    ),
    guestPassQuota: this.fb.control<number | null>(
      0,
      Validators.min(0)
    ),
    accessScope: this.fb.control<number | null>(
      null,
      Validators.required
    ),
    isPublished: this.fb.control(
      false,
      { nonNullable: true }
    )
  });

  error = computed(() =>
    this.loadError() ||
    this.saveError()
  );

  title = computed(() =>
    this.mode() === 'create'
      ? 'New Plan'
      : this.plan()?.planName ?? 'Plan Details'
  );

  subtitle = computed(() =>
    this.mode() === 'create'
      ? 'Publish a new membership plan.'
      : 'Plan details'
  );

  badgeMode = computed<'add' | 'edit' | 'view'>(() =>
    this.mode() === 'create'
      ? 'add'
      : this.mode() === 'edit'
        ? 'edit'
        : 'view'
  );

  // Reads the mode and selected plan from the URL.
  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(params => {
        const rawMode = params.get('mode');

        const mode: PlanMode =
          rawMode === 'view' || rawMode === 'edit'
            ? rawMode
            : 'create';

        const planId = Number(
          params.get('planId')
        );

        this.mode.set(mode);

        this.planId.set(
          planId > 0
            ? planId
            : null
        );

        this.loadError.set('');
        this.saveError.set('');

        if (mode === 'create') {
          this.prepareCreate();
          return;
        }

        if (planId < 1) {
          this.plan.set(null);
          this.loadError.set(
            'A plan must be selected.'
          );
          return;
        }

        if (this.plan()?.planId === planId) {
          this.applyMode();
          return;
        }

        this.loadPlan(planId);
      });
  }

  // Validates the form and creates or updates the plan.
  save(): void {
    if (this.mode() === 'view')
      return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    const plan: PlanRequestDto = {
      planName: value.planName?.trim() ?? '',
      price: value.price!,
      durationInMonths: value.durationInMonths!,
      maxFreezeDays: value.maxFreezeDays ?? 0,
      maxFreezes: value.maxFreezes ?? 0,
      guestPassQuota: value.guestPassQuota ?? 0,
      accessScope: value.accessScope!,
      isPublished: value.isPublished
    };

    if (this.mode() === 'create') {
      this.createPlan(plan);
      return;
    }

    const planId = this.planId();

    if (!planId)
      return;

    this.updatePlan(
      planId,
      plan
    );
  }

  // Switches the current plan from view to edit mode.
  editPlan(): void {
    const planId = this.planId();

    if (!planId)
      return;

    this.router.navigate(['/plans/details'], {
      queryParams: {
        mode: 'edit',
        planId
      }
    });
  }

  // Returns to the plan catalogue.
  back(): void {
    this.router.navigate(['/plans']);
  }

  // Cancels editing and returns to the catalogue.
  cancel(): void {
    this.router.navigate(['/plans']);
  }

  // Creates a new plan through the API.
  private createPlan(plan: PlanRequestDto): void {
    this.saving.set(true);
    this.saveError.set('');

    this.plansService.createPlan(plan)
      .pipe(
        take(1),
        finalize(() =>
          this.saving.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () =>
          this.router.navigate(['/plans']),
        error: error =>
          this.saveError.set(error.message)
      });
  }

  // Updates the selected plan through the API.
  private updatePlan(
    planId: number,
    plan: PlanRequestDto
  ): void {
    this.saving.set(true);
    this.saveError.set('');

    this.plansService.updatePlan(
      planId,
      plan
    )
      .pipe(
        take(1),
        finalize(() =>
          this.saving.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () =>
          this.router.navigate(['/plans']),
        error: error =>
          this.saveError.set(error.message)
      });
  }

  // Resets the form for creating a new plan.
  private prepareCreate(): void {
    this.plan.set(null);
    this.loading.set(false);

    this.form.enable({
      emitEvent: false
    });

    this.form.reset({
      planName: '',
      price: 0,
      durationInMonths: null,
      maxFreezeDays: 0,
      maxFreezes: 0,
      guestPassQuota: 0,
      accessScope: null,
      isPublished: false
    }, {
      emitEvent: false
    });
  }

  // Loads the selected plan and fills the form.
  private loadPlan(planId: number): void {
    this.loading.set(true);
    this.loadError.set('');
    this.plan.set(null);

    this.plansService.getPlanById(planId)
      .pipe(
        take(1),
        finalize(() =>
          this.loading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          const plan = response.data;

          this.plan.set(plan);

          this.form.patchValue({
            planName: plan.planName,
            price: plan.price,
            durationInMonths: plan.durationInMonths,
            maxFreezeDays: plan.maxFreezeDays,
            maxFreezes: plan.maxFreezes,
            guestPassQuota: plan.guestPassQuota,
            accessScope: this.accessScopeId(
              plan.accessScope
            ),
            isPublished: plan.isPublished
          }, {
            emitEvent: false
          });

          this.applyMode();
        },
        error: error =>
          this.loadError.set(error.message)
      });
  }

  // Enables or disables the form based on the mode.
  private applyMode(): void {
    this.form.enable({
      emitEvent: false
    });

    if (this.mode() === 'view')
      this.form.disable({
        emitEvent: false
      });
  }

  // Converts the access scope name into its numeric value.
  private accessScopeId(
    accessScope: string
  ): AccessScopeFilter | null {
    if (accessScope === 'Home Branch Only')
      return AccessScopeFilter.HomeBranchOnly;

    if (accessScope === 'All Branches')
      return AccessScopeFilter.AllBranches;

    return null;
  }
}
