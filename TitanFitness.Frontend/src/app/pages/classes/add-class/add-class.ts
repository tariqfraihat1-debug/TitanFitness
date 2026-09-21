import { Component, computed, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCalendarPlus } from '@fortawesome/free-solid-svg-icons';
import { catchError, distinctUntilChanged, forkJoin, map, of, startWith, switchMap, tap } from 'rxjs';
import { BranchDto } from '../../../apis/branches/branch.dto';
import { ClassSessionsService } from '../../../apis/class-sessions/class-sessions.service';
import { CreateClassSessionDto } from '../../../apis/class-sessions/create-class-session.dto';
import { StudioDto } from '../../../apis/studios/studio.dto';
import { StudiosService } from '../../../apis/studios/studios.service';
import { TrainerLookupItemDto } from '../../../apis/trainers/trainer-lookup-item.dto';
import { TrainersService } from '../../../apis/trainers/trainers.service';
import { AlertMessage } from '../../../shared/components/alert-message/alert-message';
import { Button } from '../../../shared/components/button/button';
import { Editor } from '../../../shared/components/editor/editor';
import { Modal } from '../../../shared/components/modal/modal';
import { SelectEditor } from '../../../shared/components/select-editor/select-editor';
import { TextareaEditor } from '../../../shared/components/textarea-editor/textarea-editor';

@Component({
  selector: 'app-add-class',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FontAwesomeModule,
    AlertMessage,
    Button,
    Editor,
    Modal,
    SelectEditor,
    TextareaEditor
  ],
  templateUrl: './add-class.html'
})
export class AddClass implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly studiosService = inject(StudiosService);
  private readonly trainersService = inject(TrainersService);
  private readonly classSessionsService = inject(ClassSessionsService);

  branches = input.required<BranchDto[]>();
  defaultBranchId = input<number | null>(null);
  defaultDate = input.required<string>();

  closed = output<void>();
  created = output<{ branchId: number; date: string }>();

  studios = signal<StudioDto[]>([]);
  trainers = signal<TrainerLookupItemDto[]>([]);
  selectedStudio = signal<StudioDto | null>(null);

  loadingOptions = signal(false);
  saving = signal(false);
  error = signal('');

  readonly scheduleIcon = faCalendarPlus;

  readonly durations = [
    { value: 30, label: '30 min' },
    { value: 45, label: '45 min' },
    { value: 60, label: '60 min' }
  ];

  readonly form = this.fb.group({
    className: ['', [Validators.required, Validators.maxLength(100)]],
    branchId: [null as number | null, Validators.required],
    trainerId: [null as number | null, Validators.required],
    studioId: [null as number | null, Validators.required],
    sessionDate: ['', Validators.required],
    startTime: ['', Validators.required],
    durationMinutes: [45 as number | null, Validators.required],
    capacityLimit: [null as number | null, [Validators.required, Validators.min(1)]],
    description: ['', [Validators.maxLength(500)]]
  });

  readonly selectFields = computed(() => [
    {
      label: 'Branch',
      control: this.form.controls.branchId,
      options: this.branches().map(branch => ({
        value: branch.branchId,
        label: branch.name
      })),
      placeholder: 'Select a branch',
      requiredMark: true,
      errorMessage: 'Branch is required.'
    },
    {
      label: 'Trainer / Instructor',
      control: this.form.controls.trainerId,
      options: this.trainers().map(trainer => ({
        value: trainer.trainerId,
        label: trainer.specialty
          ? `${trainer.name} — ${trainer.specialty}`
          : trainer.name
      })),
      placeholder: 'Select an instructor',
      requiredMark: true,
      errorMessage: 'Trainer is required.'
    },
    {
      label: 'Studio / Room',
      control: this.form.controls.studioId,
      options: this.studios().map(studio => ({
        value: studio.studioId,
        label: studio.name
      })),
      placeholder: 'Assign a room',
      requiredMark: true,
      errorMessage: 'Studio is required.'
    }
  ]);

  // Sets default values and starts watching branch and studio changes.
  ngOnInit(): void {
    this.form.patchValue({
      branchId: this.defaultBranchId(),
      sessionDate: this.defaultDate()
    }, { emitEvent: false });

    this.watchBranch();
    this.watchStudio();
  }

  // Updates the selected class duration.
  selectDuration(duration: number): void {
    this.form.controls.durationMinutes.setValue(duration);
    this.form.controls.durationMinutes.markAsTouched();
  }

  // Validates and creates the class session.
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    const session: CreateClassSessionDto = {
      className: value.className!.trim(),
      branchId: value.branchId!,
      studioId: value.studioId!,
      trainerId: value.trainerId!,
      sessionDate: value.sessionDate!,
      startTime: value.startTime!,
      durationMinutes: value.durationMinutes!,
      capacityLimit: value.capacityLimit!,
      description: value.description?.trim() || null
    };

    this.saving.set(true);
    this.error.set('');

    this.classSessionsService.createClassSession(session)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.saving.set(false);

          this.created.emit({
            branchId: session.branchId,
            date: session.sessionDate
          });
        },
        error: error => {
          this.saving.set(false);
          this.error.set(error.message);
        }
      });
  }

  // Loads studios and trainers when the branch changes.
  private watchBranch(): void {
    this.form.controls.branchId.valueChanges
      .pipe(
        startWith(this.form.controls.branchId.value),
        distinctUntilChanged(),

        tap(() => {
          this.form.controls.studioId.setValue(null, { emitEvent: false });
          this.form.controls.trainerId.setValue(null, { emitEvent: false });
          this.form.controls.capacityLimit.setValue(null);

          this.studios.set([]);
          this.trainers.set([]);
          this.selectedStudio.set(null);

          this.updateCapacityValidator(null);
          this.error.set('');
        }),

        switchMap(branchId => {
          if (!branchId)
            return of({
              studios: [] as StudioDto[],
              trainers: [] as TrainerLookupItemDto[]
            });

          this.loadingOptions.set(true);

          return forkJoin({
            studios: this.studiosService
              .getStudios(branchId)
              .pipe(
                map(response => response.data)
              ),

            trainers: this.trainersService
              .getAvailableTrainers(branchId)
              .pipe(
                map(response => response.data)
              )
          })
            .pipe(
              catchError(error => {
                this.error.set(error.message);

                return of({
                  studios: [] as StudioDto[],
                  trainers: [] as TrainerLookupItemDto[]
                });
              })
            );
        }),

        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        this.loadingOptions.set(false);
        this.studios.set(response.studios);
        this.trainers.set(response.trainers);
      });
  }

  // Updates studio details and capacity rules when a studio changes.
  private watchStudio(): void {
    this.form.controls.studioId.valueChanges
      .pipe(
        startWith(this.form.controls.studioId.value),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(studioId => {
        const studio = this.studios()
          .find(studio => studio.studioId === studioId) ?? null;

        this.selectedStudio.set(studio);
        this.updateCapacityValidator(studio);
      });
  }

  // Applies the allowed capacity based on the selected studio.
  private updateCapacityValidator(studio: StudioDto | null): void {
    const validators = [
      Validators.required,
      Validators.min(1)
    ];

    if (studio)
      validators.push(
        Validators.max(studio.capacity)
      );

    this.form.controls.capacityLimit
      .setValidators(validators);

    this.form.controls.capacityLimit
      .updateValueAndValidity({
        emitEvent: false
      });
  }
}
