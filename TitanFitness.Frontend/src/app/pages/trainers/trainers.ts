import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { catchError, finalize, of } from 'rxjs';
import { BranchDto } from '../../apis/branches/branch.dto';
import { BranchesService } from '../../apis/branches/branches.service';
import { TrainerListItemDto } from '../../apis/trainers/trainer-list-item.dto';
import { TrainersService } from '../../apis/trainers/trainers.service';
import { AlertMessage } from '../../shared/components/alert-message/alert-message';
import { BranchSelect } from '../../shared/components/branch-select/branch-select';
import { Button } from '../../shared/components/button/button';
import { ContentHeader } from '../../shared/components/content-header/content-header';
import { Loading } from '../../shared/components/loading/loading';
import {
  TrainerAction,
  TrainerTable
} from './components/trainer-table/trainer-table';

@Component({
  selector: 'app-trainers',
  standalone: true,
  imports: [
    FontAwesomeModule,
    AlertMessage,
    BranchSelect,
    Button,
    ContentHeader,
    Loading,
    TrainerTable
  ],
  templateUrl: './trainers.html'
})
export class Trainers implements OnInit {
  private readonly trainersService = inject(TrainersService);
  private readonly branchesService = inject(BranchesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  trainers = signal<TrainerListItemDto[]>([]);
  branches = signal<BranchDto[]>([]);

  branchId = signal<number | null>(null);
  search = signal('');
  page = signal(1);
  pageSize = signal(4);
  totalPages = signal(0);
  totalCount = signal(0);

  private readonly branchesLoading = signal(true);
  private readonly trainersLoading = signal(true);

  loading = computed(() =>
    this.branchesLoading() ||
    this.trainersLoading()
  );

  branchError = signal('');
  trainerError = signal('');

  error = computed(() =>
    this.branchError() ||
    this.trainerError()
  );

  readonly addIcon = faPlus;

  // Reads filters from the URL and loads trainers.
  ngOnInit(): void {
    this.loadBranches();

    this.route.queryParamMap
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(params => {
        const branchId = params.get('branchId');

        this.branchId.set(
          branchId
            ? Number(branchId)
            : null
        );

        this.search.set(
          params.get('search') ?? ''
        );

        this.page.set(
          Number(params.get('page')) || 1
        );

        this.loadTrainers();
      });
  }

  // Updates the selected branch filter.
  onBranchChange(branchId: number | null): void {
    this.router.navigate(['/trainers'], {
      queryParams: {
        branchId,
        page: 1
      },
      queryParamsHandling: 'merge'
    });
  }

  // Changes the current trainer page.
  onPageChange(page: number): void {
    this.router.navigate(['/trainers'], {
      queryParams: { page },
      queryParamsHandling: 'merge'
    });
  }

  // Opens the trainer details page in create mode.
  addTrainer(): void {
    this.router.navigate(['/trainers/details'], {
      queryParams: {
        mode: 'create'
      }
    });
  }

  // Opens the selected trainer in view or edit mode.
  onTrainerAction(event: {
    action: TrainerAction;
    trainerId: number;
  }): void {
    if (event.action === 'viewTrainer') {
      this.router.navigate(['/trainers/details'], {
        queryParams: {
          mode: 'view',
          trainerId: event.trainerId
        }
      });
      return;
    }

    this.router.navigate(['/trainers/details'], {
      queryParams: {
        mode: 'edit',
        trainerId: event.trainerId
      }
    });
  }

  // Loads branches used by the directory filter.
  private loadBranches(): void {
    this.branchesLoading.set(true);
    this.branchError.set('');

    this.branchesService.getBranches()
      .pipe(
        catchError(error => {
          this.branchError.set(
            error.message
          );

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

  // Loads the filtered and paged trainer list.
  private loadTrainers(): void {
    this.trainersLoading.set(true);
    this.trainerError.set('');
    this.trainers.set([]);
    this.totalPages.set(0);
    this.totalCount.set(0);

    this.trainersService.getTrainers(
      this.branchId(),
      this.normalizedSearch(),
      this.page()
    )
      .pipe(
        catchError(error => {
          this.trainerError.set(
            error.message
          );

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
          this.trainersLoading.set(false)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        this.trainers.set(
          response.data.items
        );

        this.pageSize.set(
          response.data.pageSize
        );

        this.totalCount.set(
          response.data.totalCount
        );

        this.totalPages.set(
          response.data.totalPages
        );
      });
  }

  // Converts trainer numbers like TR-0001 into searchable IDs.
  private normalizedSearch(): string {
    const search = this.search().trim();

    const trainerNumber =
      search.match(/^#?TR-(\d+)$/i);

    return trainerNumber
      ? trainerNumber[1]
      : search;
  }
}
