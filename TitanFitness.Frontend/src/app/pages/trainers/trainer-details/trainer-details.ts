import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCheck, faPen } from '@fortawesome/free-solid-svg-icons';
import { finalize, take } from 'rxjs';
import { BranchDto } from '../../../apis/branches/branch.dto';
import { BranchesService } from '../../../apis/branches/branches.service';
import { TrainerBaseDto } from '../../../apis/trainers/trainer-base.dto';
import { TrainerDetailsDto } from '../../../apis/trainers/trainer-details.dto';
import { TrainersService } from '../../../apis/trainers/trainers.service';
import { AlertMessage } from '../../../shared/components/alert-message/alert-message';
import { BackNavigation } from '../../../shared/components/back-navigation/back-navigation';
import { Button } from '../../../shared/components/button/button';
import { Editor } from '../../../shared/components/editor/editor';
import { Loading } from '../../../shared/components/loading/loading';
import { ModeBadge } from '../../../shared/components/mode-badge/mode-badge';
import { SelectEditor } from '../../../shared/components/select-editor/select-editor';

type TrainerMode = 'create' | 'view' | 'edit';
type TrainerTextControl = 'name' | 'specialty' | 'email' | 'phone';
type TrainerInputType = 'text' | 'email' | 'tel';

type TrainerField =
  | {
    key: string;
    kind: 'text';
    control: TrainerTextControl;
    label: string;
    type: TrainerInputType;
    placeholder: string;
    requiredMark: boolean;
    errorMessage: string;
  }
  | {
    key: string;
    kind: 'branch';
    label: string;
  }
  | {
    key: string;
    kind: 'status';
    label: string;
  };

@Component({
  selector: 'app-trainer-details',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FontAwesomeModule,
    AlertMessage,
    BackNavigation,
    Button,
    Editor,
    Loading,
    ModeBadge,
    SelectEditor
  ],
  templateUrl: './trainer-details.html'
})
export class TrainerDetails implements OnInit {
  private readonly trainersService = inject(TrainersService);
  private readonly branchesService = inject(BranchesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  branches = signal<BranchDto[]>([]);
  trainer = signal<TrainerDetailsDto | null>(null);

  mode = signal<TrainerMode>('create');
  trainerId = signal<number | null>(null);

  branchesLoading = signal(true);
  trainerLoading = signal(false);
  saving = signal(false);

  branchError = signal('');
  trainerError = signal('');
  saveError = signal('');

  readonly editIcon = faPen;
  readonly saveIcon = faCheck;

  readonly fields: TrainerField[] = [
    {
      key: 'name',
      kind: 'text',
      control: 'name',
      label: 'Trainer name',
      type: 'text',
      placeholder: 'e.g., Sarah Jenkins',
      requiredMark: true,
      errorMessage: 'Trainer name is required and cannot exceed 100 characters.'
    },
    {
      key: 'specialty',
      kind: 'text',
      control: 'specialty',
      label: 'Specialty',
      type: 'text',
      placeholder: 'e.g., HIIT / Strength',
      requiredMark: false,
      errorMessage: 'Specialty cannot exceed 100 characters.'
    },
    {
      key: 'branch',
      kind: 'branch',
      label: 'Branch'
    },
    {
      key: 'email',
      kind: 'text',
      control: 'email',
      label: 'Email',
      type: 'email',
      placeholder: 'name@titanfitness.com',
      requiredMark: false,
      errorMessage: 'Enter a valid email of no more than 100 characters.'
    },
    {
      key: 'phone',
      kind: 'text',
      control: 'phone',
      label: 'Phone',
      type: 'tel',
      placeholder: '+1 (555) 000-0000',
      requiredMark: false,
      errorMessage: 'Phone cannot exceed 20 characters.'
    },
    {
      key: 'status',
      kind: 'status',
      label: 'Status'
    }
  ];

  readonly form = this.fb.group({
    name: this.fb.control<string | null>('', [
      Validators.required,
      Validators.maxLength(100)
    ]),
    branchId: this.fb.control<number | null>(
      null,
      Validators.required
    ),
    specialty: this.fb.control<string | null>(
      '',
      Validators.maxLength(100)
    ),
    email: this.fb.control<string | null>('', [
      Validators.email,
      Validators.maxLength(100)
    ]),
    phone: this.fb.control<string | null>(
      '',
      Validators.maxLength(20)
    ),
    isActive: this.fb.control(
      false,
      { nonNullable: true }
    )
  });

  loading = computed(() =>
    this.branchesLoading() ||
    this.trainerLoading()
  );

  error = computed(() =>
    this.branchError() ||
    this.trainerError() ||
    this.saveError()
  );

  branchOptions = computed(() =>
    this.branches().map(branch => ({
      value: branch.branchId,
      label: branch.name
    }))
  );

  title = computed(() =>
    this.mode() === 'create'
      ? 'New Trainer'
      : this.trainer()?.name ?? 'Trainer Details'
  );

  subtitle = computed(() =>
    this.mode() === 'create'
      ? 'Add a trainer to the roster.'
      : this.trainerId()
        ? `Trainer ${this.trainerNumber(this.trainerId()!)}`
        : ''
  );

  badgeMode = computed<'add' | 'edit' | 'view'>(() =>
    this.mode() === 'create'
      ? 'add'
      : this.mode() === 'edit'
        ? 'edit'
        : 'view'
  );

  // Reads the trainer mode and ID from the URL.
  ngOnInit(): void {
    this.loadBranches();

    this.route.queryParamMap
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(params => {
        const rawMode = params.get('mode');

        const mode: TrainerMode =
          rawMode === 'view' || rawMode === 'edit'
            ? rawMode
            : 'create';

        const trainerId = Number(
          params.get('trainerId')
        );

        this.mode.set(mode);

        this.trainerId.set(
          trainerId > 0
            ? trainerId
            : null
        );

        this.saveError.set('');
        this.trainerError.set('');

        if (mode === 'create') {
          this.prepareCreate();
          return;
        }

        if (trainerId < 1) {
          this.trainer.set(null);

          this.trainerError.set(
            'A trainer must be selected.'
          );

          return;
        }

        if (this.trainer()?.trainerId === trainerId) {
          this.applyMode();
          return;
        }

        this.loadTrainer(trainerId);
      });
  }

  // Validates the form and creates or updates the trainer.
  save(): void {
    if (this.mode() === 'view')
      return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    const trainer: TrainerBaseDto = {
      name: value.name?.trim() ?? '',
      branchId: value.branchId!,
      specialty: value.specialty?.trim() || null,
      email: value.email?.trim() || null,
      phone: value.phone?.trim() || null,
      isActive: value.isActive
    };

    if (this.mode() === 'create') {
      this.saving.set(true);
      this.saveError.set('');

      this.trainersService.createTrainer(trainer)
        .pipe(
          take(1),
          finalize(() =>
            this.saving.set(false)
          ),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe({
          next: () =>
            this.router.navigate(['/trainers']),
          error: error =>
            this.saveError.set(error.message)
        });

      return;
    }

    const trainerId = this.trainerId();

    if (!trainerId)
      return;

    this.saving.set(true);
    this.saveError.set('');

    this.trainersService.updateTrainer(
      trainerId,
      trainer
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
          this.router.navigate(['/trainers']),
        error: error =>
          this.saveError.set(error.message)
      });
  }

  // Switches the current trainer from view to edit mode.
  editTrainer(): void {
    const trainerId = this.trainerId();

    if (!trainerId)
      return;

    this.router.navigate(['/trainers/details'], {
      queryParams: {
        mode: 'edit',
        trainerId
      }
    });
  }

  // Returns to the trainer directory.
  back(): void {
    this.router.navigate(['/trainers']);
  }

  // Cancels editing and returns to the directory.
  cancel(): void {
    this.router.navigate(['/trainers']);
  }

  // Formats the trainer ID for display.
  trainerNumber(trainerId: number): string {
    return `#TR-${String(trainerId).padStart(4, '0')}`;
  }

  // Resets the form for creating a new trainer.
  private prepareCreate(): void {
    this.trainer.set(null);
    this.trainerLoading.set(false);

    this.form.enable({
      emitEvent: false
    });

    this.form.reset({
      name: '',
      branchId: null,
      specialty: '',
      email: '',
      phone: '',
      isActive: false
    }, {
      emitEvent: false
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

  // Loads the selected trainer and fills the form.
  private loadTrainer(trainerId: number): void {
    this.trainerLoading.set(true);
    this.trainerError.set('');
    this.trainer.set(null);

    this.trainersService.getTrainerById(trainerId)
      .pipe(
        take(1),
        finalize(() =>
          this.trainerLoading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          const trainer = response.data;

          this.trainer.set(trainer);

          this.form.patchValue({
            name: trainer.name,
            branchId: trainer.branchId,
            specialty: trainer.specialty,
            email: trainer.email,
            phone: trainer.phone,
            isActive: trainer.isActive
          }, {
            emitEvent: false
          });

          this.applyMode();
        },
        error: error =>
          this.trainerError.set(error.message)
      });
  }

  // Loads branches for the trainer branch selector.
  private loadBranches(): void {
    this.branchesLoading.set(true);
    this.branchError.set('');

    this.branchesService.getBranches()
      .pipe(
        take(1),
        finalize(() =>
          this.branchesLoading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response =>
          this.branches.set(response.data),
        error: error =>
          this.branchError.set(error.message)
      });
  }
}
