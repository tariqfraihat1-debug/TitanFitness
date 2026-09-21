import { Component, input, output } from '@angular/core';
import { BranchDto } from '../../../../../apis/branches/branch.dto';
import { BranchSelect } from '../../../../../shared/components/branch-select/branch-select';
import { Button } from '../../../../../shared/components/button/button';

@Component({
  selector: 'app-schedule-filters',
  standalone: true,
  imports: [BranchSelect, Button],
  templateUrl: './schedule-filters.html'
})
export class ScheduleFilters {
  branches = input.required<BranchDto[]>();
  branchId = input<number | null>(null);
  date = input.required<string>();

  branchChanged = output<number | null>();
  dateChanged = output<string>();
  addClass = output<void>();

  onDateChange(event: Event): void {
    this.dateChanged.emit(
      (event.target as HTMLInputElement).value
    );
  }
}
