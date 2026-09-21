import { Component, DestroyRef, inject, input, output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faMagnifyingGlass } from '@fortawesome/free-solid-svg-icons';
import { debounceTime, distinctUntilChanged, map } from 'rxjs';
import { MemberListItemDto } from '../../../apis/members/member-list-item.dto';
import { EmptyCard } from '../empty-card/empty-card';
import { MemberSummaryCard } from '../member-summary-card/member-summary-card';

@Component({
  selector: 'app-member-search',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FontAwesomeModule,
    EmptyCard,
    MemberSummaryCard
  ],
  templateUrl: './member-search.html'
})
export class MemberSearch {
  private readonly destroyRef = inject(DestroyRef);

  members = input.required<MemberListItemDto[]>();
  loading = input(false);
  searched = input(false);

  searchChanged = output<string>();
  memberSelected = output<MemberListItemDto>();

  readonly searchIcon = faMagnifyingGlass;
  readonly searchControl = new FormControl('', { nonNullable: true });

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        map(value => value.trim()),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(value =>
        this.searchChanged.emit(value)
      );
  }
}
